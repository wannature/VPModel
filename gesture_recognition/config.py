# -*- coding: utf-8 -*-
import tensorflow as tf

CONFIG ={
        'log_dir': 'C:/Users/Setella/Desktop/gesture_recognition0727/log/',
        'result': 'C:/Users/Setella/Desktop/gesture_recognition0727/result/',
        'source_data_dir':'C:/Users/Setella/Desktop/gesture_recognition0727/data/gestures/',
        'train_data_dir':'C:/Users/Setella/Desktop/gesture_recognition0727/data/train/gesture_data/',
        'gesture_types':'1,2,3,4',
       # 'train_data_json':'./data/data_label.json',
       'train_data_json':'C:/Users/Setella/Desktop/gesture_recognition0727/data/data_label.json',
        'test_data_json':'C:/Users/Setella/Desktop/gesture_recognition0727/data/data_label_test.json',
        # 'test_data_dir':'C:/Users/Setella/Desktop/gesture_recognition0727/data/test/gesture_data/',
        'test_data_dir':'C:/Users/Setella/img/',
        'train_data_h5':'C:/Users/Setella/Desktop/gesture_recognition0727/data/gestures_feature_vgg.h5',
        'svm_path':'C:/Users/Setella/Desktop/gesture_recognition0727/log/svm/svm_model.m',
        'gpu_id':1,
        'att_dnn': {
                    'model': {
                        'gesture_dim': 4096,
                        'hidden_dim': 20,
                        'prediction_dim': 4,
                        'cell_dim': 20
                    },
                    'train': {
                        'batch_size': 85,
                        'reg_coeff': 1e-5,
                        'learning_rate': 0.001,
                        'loss_value': 0.00003
                    }
                },
        'svm': {
                'model':{
                    'cell_size':8,
                    'bin_size':8,
                    'feature_size':100
                },
                    'train': {
                        'gamma': 0.001,
                        'C': 100.
                    }
        }
}


def get(model):
    config = {}
    if model is 'att_dnn':
        config['model'] = CONFIG[model]['model']
    elif model is 'svm':
        config['model'] = CONFIG[model]['model']
    config['model'] = CONFIG[model]['model']
    config['train'] = CONFIG[model]['train']
    config['log_dir']=CONFIG['log_dir']
    config['result'] = CONFIG['result']
    config['train_data_dir']=CONFIG['train_data_dir']
    config['train_data_json']= CONFIG['train_data_json']
    config['test_data_json']= CONFIG ['test_data_json']
    config['test_data_dir'] = CONFIG['test_data_dir']
    config['train_data_h5']=CONFIG['train_data_h5']
    config['svm_path'] = CONFIG['svm_path']
    sess_config = tf.ConfigProto()
    sess_config.gpu_options.allow_growth =True
    sess_config.gpu_options.visible_device_list = str(CONFIG['gpu_id'])
    config['session'] = sess_config
    return config