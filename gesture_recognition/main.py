# -*- coding: utf-8 -*-
import tensorflow as tf
import pandas as pd
from pandas import Series
import hog_feature
import os
import config as cfg
import json
import argparse
from model.att_dnn import ATT_DNN
from sklearn import svm
from sklearn.externals import joblib
import util.dataset as data
from util.preprocess import VGGExtractor
import object_segmentation as osg
import cv2
import tables
import matplotlib.pyplot as plt

def get_image_and_label(image_path, json_path):
    with open(json_path, 'r') as load_f:
        load_dict = json.load(load_f)
    length = len(load_dict)
    images = []
    labels = []
    for i in range(0, length):
        images.append(image_path + load_dict[i]['image'])
        labels.append(load_dict[i]["label"])
    return images, labels

def get_vgg_features(config, images):
    gesture_feature=[]
    with tf.Graph().as_default(), tf.Session(config=config['session']) as sess:
        vgg_extrator = VGGExtractor(sess)
        for image in images:
            img = osg.segmentation(image)
            img = cv2.resize(img, (224, 224), 0, 0)
            img = img.reshape((1,224,224,3))
            gesture_feature.append (vgg_extrator.extract_image(img))
        # h5file = tables.open_file(
        #     cfg.CONFIG['train_data_h5'], 'w', 'Extracted gesture features.')
        # h5file.create_array('/', 'vgg', gesture_feature)
        # h5file.close()
        return gesture_feature

def train(epoch, dataset, config, model_name):
    model_config = config['model']
    train_config = config['train']
    sess_config = config['session']
    
    with tf.Graph().as_default():
        model = ATT_DNN(model_config)
        model.build_inference()
        model.build_loss(train_config['reg_coeff'])
        model.build_train(train_config['learning_rate'])
        log_dir=config['log_dir']+"_"+str(model_name)
        with tf.Session(config=sess_config) as sess:
            sum_dir = os.path.join(log_dir, 'summary')
            # create event file for graph
            if not os.path.exists(sum_dir):
                summary_writer = tf.summary.FileWriter(sum_dir, sess.graph)
                summary_writer.close()
            summary_writer = tf.summary.FileWriter(sum_dir)

            ckpt_dir = os.path.join(log_dir, 'checkpoint')
            ckpt_path = tf.train.latest_checkpoint(ckpt_dir)
            saver = tf.train.Saver()
            if ckpt_path:
                print('load checkpoint {}.'.format(ckpt_path))
                saver.restore(sess, ckpt_path)
            else:
                print('no checkpoint.')
                if not os.path.exists(ckpt_dir):
                    os.makedirs(ckpt_dir)
                sess.run(tf.global_variables_initializer())

            stats_dir = os.path.join(log_dir, 'stats')
            stats_path = os.path.join(stats_dir, 'train.json')
            if os.path.exists(stats_path):
                print('load stats file {}.'.format(stats_path))
                stats = pd.read_json(stats_path, 'records')
            else:
                print('no stats file.')
                if not os.path.exists(stats_dir):
                    os.makedirs(stats_dir)
                stats = pd.DataFrame(columns=['epoch', 'loss'])

            # train iterate over batch
            total_loss = 0
            for i in range(10):
                while dataset.has_train_batch:
                    gesture_batch, labels  = dataset.get_train_batch()
                    _, loss= sess.run(
                        [model.train, model.loss], feed_dict = {
                        model.gesture_feature: gesture_batch,
                        model.ground_truth: labels
                    })
                    total_loss += loss
                print("epoch " + str(epoch * 10) + " loss: " + str(total_loss))
                total_loss = 0
                dataset.reset_train()

            print ("epoch " +str(epoch * 10) + " loss: " + str(total_loss))
            summary = tf.Summary()
            summary.value.add(tag='train/loss', simple_value=float(total_loss))
            summary_writer.add_summary(summary, epoch * 5000)
            record = Series([epoch * 10, total_loss,], ['epoch', 'loss'])
            stats = stats.append(record, ignore_index=True)
            saver.save(sess, os.path.join(ckpt_dir, 'model.ckpt'), epoch * 10)
            loss = total_loss
            print('\n[TRAIN] epoch {}, loss {:.7f}.\n'.format(epoch * 10, loss))
            stats.to_json(stats_path, 'records')
            return total_loss

def train_svm(train_images,train_labels,config):
    train_image_vectors=[]
    hog = hog_feature.Hog_descriptor
    flatten = lambda x: [y for l in x for y in flatten(l)] if type(x) is list else [x]
    for image in train_images:
        img = hog.get_hog_feature(image,config)
        train_image_vectors.append(flatten(img))
    svm_model = svm.SVC(gamma=float(config['train']['gamma']), C=float(config['train']['C']))
    svm_model.fit(train_image_vectors, train_labels)
    
    prediction = svm_model.predict(train_image_vectors)
    print('\n The accuracy is {}%.\n'.format(accuracy(train_labels, prediction) * 100))
    svm_path=config['log_dir']+'svm/'
    joblib.dump(svm_model, config['svm_path'])
    print("Done\n")
    
def test_svm(test_images,test_labels,config):
    train_image_vectors = []
    hog = hog_feature.Hog_descriptor
    flatten = lambda x: [y for l in x for y in flatten(l)] if type(x) is list else [x]
    for image in test_images:
        img = hog.get_hog_feature(image, config)
        train_image_vectors.append(flatten(img))
    svm_model = joblib.load(config['svm_path'])
    
    
    prediction = svm_model.predict(train_image_vectors)
    print(prediction)
    print('\n The test accuracy is {}%.\n'.format(accuracy(test_labels, prediction) * 100))
    
def accuracy (ground_truth,prediction):
    correct_preditction=0
    if len(ground_truth)!=len(prediction):
        print ("Please confirm the length!")
    else:
        for i in range (len(prediction)):
            if ground_truth[i]==prediction[i]:
                correct_preditction+=1
    return correct_preditction/len(prediction)

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--mode', required=True,
                        help='train/test')
    parser.add_argument('--model', required=True,
                        help='att_dnn/svm')
    args = parser.parse_args()

    config = cfg.get( args.model)
    if args.model == 'att_dnn':
        if args.mode == 'train':
            train_images,train_labels=get_image_and_label(config['train_data_dir'],
                                                          config['train_data_json'])
            # train_features=get_vgg_features(config, train_images)
            train_features = tables.open_file(config['train_data_h5'])
            dataset = data.DATA(
                config['train']['batch_size'], train_features, train_labels, 'train')
            for epoch in range(0, 150):
                train(epoch, dataset, config, args.model)
    if args.model == 'svm':
        if args.mode == 'train':
            train_images, train_labels = get_image_and_label(config['train_data_dir'],
                                                             config['train_data_json'])
            train_svm(train_images,train_labels, config)
        if args.mode == 'test':
            test_images, test_labels = get_image_and_label(config['test_data_dir'],
                                                             config['test_data_json'])
            test_svm(test_images, test_labels, config)


if __name__ == '__main__':
    main()

 