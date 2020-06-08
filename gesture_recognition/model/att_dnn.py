# -*- coding: utf-8 -*-
import numpy as np
import tensorflow as tf

class ATT_DNN(object):
    def __init__(self, config):
        """Init model."""
        self.gesture_feature=None
        self.gesture_dim=config ['gesture_dim']
        self.cell_dim = config ['cell_dim']
        self.prediction_dim = config ['prediction_dim']
        self.hidden_dim=config ['hidden_dim']
        self.logit = None
        self.prediction = None
        self.loss = None
        self.train = None
        self.hidden_feature = None

    def build_inference(self):
        with tf.name_scope('input'):
            self.gesture_feature = tf.placeholder(
                tf.float32, [None,  self.gesture_dim ], 'gesture_feature')


        with tf.variable_scope('output'):
            W = tf.get_variable(
                'W', [self.gesture_dim , self.prediction_dim],
                regularizer=tf.nn.l2_loss)
            b = tf.get_variable('b', [self.prediction_dim])
            self.logit = tf.nn.softmax(
	            tf.nn.xw_plus_b(self.gesture_feature, W, b), name='logit')
            self.prediction = tf.argmax(self.logit, axis=1, name='prediction')

    def build_loss(self, reg_coeff):
        with tf.name_scope('ground_truth'):
            self.ground_truth = tf.placeholder(
                tf.float32, [None,1], 'ground_truth')

        with tf.name_scope('loss'):
            cross_entropy = tf.reduce_mean(-tf.reduce_sum(self.ground_truth * tf.log(self.logit),
	                                                      reduction_indices=[1]))
            reg_loss = tf.add_n(
                tf.get_collection(tf.GraphKeys.REGULARIZATION_LOSSES), name='reg_loss')
            self.loss = cross_entropy
                        #+ reg_loss

    def build_train(self, learning_rate):
        with tf.variable_scope('train'):
            optimizer = tf.train.AdamOptimizer(learning_rate)
            self.train = optimizer.minimize(self.loss)

    def attend(self, target, sources, name=None):
        with tf.name_scope(name, 'attend'):
            weight = tf.nn.softmax(tf.reduce_sum(
                tf.expand_dims(target, 1) * sources, 2))
            att = tf.reduce_sum(
                tf.expand_dims(weight, 2) * sources, 1)
            return weight, att

