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
import time
from PIL import ImageGrab
from collections import Counter
import socket
from PIL import Image,ImageDraw,ImageFile
import numpy
import pytesseract
import imagehash
import collections


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

def get_image(image_path):
    length = 1
    images = []
    startpoint = 0
    for i in range(startpoint, startpoint+length):
        images.append(image_path)
    startpoint = startpoint + length
    return images

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
    
# def test_svm(test_images,test_labels,config):
def test_svm(test_images,config):
    train_image_vectors = []
    hog = hog_feature.Hog_descriptor
    flatten = lambda x: [y for l in x for y in flatten(l)] if type(x) is list else [x]
    print(test_images)
    for image in test_images:
        img = hog.get_hog_feature(image, config)
        train_image_vectors.append(flatten(img))
    svm_model = joblib.load(config['svm_path'])
    
    prediction = svm_model.predict(train_image_vectors)
    # print(prediction)
    return prediction
    # print('\n The test accuracy is {}%.\n'.format(accuracy(test_labels, prediction) * 100))
    
def accuracy (ground_truth,prediction):
    correct_preditction=0
    if len(ground_truth)!=len(prediction):
        print ("Please confirm the length!")
    else:
        for i in range (len(prediction)):
            if ground_truth[i]==prediction[i]:
                correct_preditction+=1
    return correct_preditction/len(prediction)

def compare_image_with_hash(image_file1,image_file2, max_dif):
    ImageFile.LOAD_TRUNCATED_IMAGES = True
    hash_1 = None
    hash_2 = None
    with open(image_file1, 'rb') as fp:
        hash_1 = imagehash.average_hash(Image.open(fp))
    with open(image_file2, 'rb') as fp:
        hash_2 = imagehash.average_hash(Image.open(fp))
    dif = hash_1 - hash_2
    if dif < 0:
        dif = -dif
    if dif <= max_dif:
        print("the photo is blank!")
        return True
    else:
        return False

def main():
    config = cfg.get("svm")
    # test_images, test_labels = get_image_and_label(config['test_data_dir'],config['test_data_json'])
    # test_images = get_image(config['test_data_dir'])
    # test_svm(test_images, test_labels, config)
    #设置端口号，主机号
    port=8781
    host='192.168.1.12'
    addr=(host,port)
    server_socket=socket.socket(socket.AF_INET,socket.SOCK_DGRAM)
    server_socket.bind(addr)
    receive_data,client_address=server_socket.recvfrom(1024)
    print("client:",client_address,":",receive_data.decode())
    start = input("click any character to start:")
    #time.sleep(8)
    isopened = False
    istaked = False
    isplaying = False
    while True:
        nowtime_head = time.strftime('%Y_%m_%d_%H_%M_%S',time.localtime(time.time()))
        nowtime_secs = (time.time() - int(time.time())) * 1000
        nowtime = "%s_%03d"%(nowtime_head,nowtime_secs)
        print(nowtime)
        img = ImageGrab.grab()
        im = img.crop((0,0,2560,1600))
        im.save(r'img\%s.png' %(nowtime))
        imgstr = 'C:/Users/Setella/img/'+nowtime+'.png'
        if (compare_image_with_hash(imgstr,"C:/Users/Setella/blank.png",0) == True):
            break
    gesture = []
    while True:
        test_images = []
        for i in range (0,3):
            nowtime_head = time.strftime('%Y_%m_%d_%H_%M_%S',time.localtime(time.time()))
            nowtime_secs = (time.time() - int(time.time())) * 1000
            nowtime = "%s_%03d"%(nowtime_head,nowtime_secs)
            print(nowtime)
            img = ImageGrab.grab()
            im = img.crop((0,0,2560,1600))
            im.save(r'img\%s.png' %(nowtime))
            imgstr = 'C:/Users/Setella/img/'+nowtime+'.png'
            if (compare_image_with_hash(imgstr,"C:/Users/Setella/blank.png",0) == False):
                test_images.append('C:/Users/Setella/img/'+nowtime+'.png')
        if (len(test_images)):
            gesture = test_svm(test_images, config)
            print(gesture)
            five_gesture = Counter(gesture)
            common_gesture = five_gesture.most_common(3)[0][0]
            print(common_gesture)
            if (common_gesture == '1' and isopened == True and isplaying == False):
                send_data = "play"
                print('play')
                server_socket.sendto(send_data.encode(),client_address)
                time.sleep(6)
                isplaying = True
                print('send success!')
            elif (common_gesture == '2' and isopened == True):
                send_data = "end"
                server_socket.sendto(send_data.encode(),client_address)
                time.sleep(2)
                send_data = "take"
                server_socket.sendto(send_data.encode(),client_address)
                time.sleep(3)
                send_data = "start"
                server_socket.sendto(send_data.encode(),client_address)
                time.sleep(7)
                isplaying = False
                print('take')
                print('send success!')
            elif (common_gesture == '3' and isopened == True):
                send_data = "close"
                print('close')
                server_socket.sendto(send_data.encode(),client_address)
                print('send success!')
                isopened = False
                isplaying = False
                time.sleep(3)
            elif (common_gesture == '4' and isopened == False):
                send_data = "open"
                print('open')
                server_socket.sendto(send_data.encode(),client_address)
                print('send success!')
                isopened = True
                isplaying = False
                time.sleep(3)
        else:
            continue
    server_socket.close()


if __name__ == '__main__':
    main()

 