# -*- coding: utf-8 -*-
"""数据batch"""
import numpy as  np
import math

class DATA(object):
    def __init__(self, batch_size, data, labels, mode):
        if mode is 'train':
            self.train_data=data
        else:
            self.test_data = data
        self.train_batch_size = batch_size
        self.current_batch_index=0
        self.test_example_idx=0
        self.has_train_batch=True
        self.has_test_example=True
        self.sum_total = len(labels)
        self.labels=labels
        self.train_batch_total=math.ceil(self.sum_total//self.train_batch_size)


    def reset_train(self):

        self.has_train_batch = True
        self.current_batch_index = 0


    def reset_test(self):

        self.test_example_idx = 0
        self.has_test_example = True


    def get_train_batch(self):
        vgg_batch=[]
        start = self.current_batch_index * self.train_batch_size
        end = start + self.train_batch_size
        if end >= self.sum_total:
            self.has_train_batch = False
            end = self.sum_total
            
        for i in range(start, end):
            vgg_batch.append(self.train_data.root.vgg[i][0])
        
        labels = np.expand_dims(self.labels[start:end], axis=1)
        return vgg_batch, labels


    def get_test_example(self):

        test_data = self.test_data[self.test_example_idx]
        self.test_example_idx += 1
        if self.test_example_idx == self.sum_total:
            self.has_test_example = False
        return self.test_example_idx-1, test_data,





