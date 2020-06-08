"""Preprocess the data for model."""
import os
import inspect
import csv

from PIL import Image
import skimage.io
import scipy
import tensorflow as tf
import pandas as pd
from .vgg16 import Vgg16
import numpy as np

class VGGExtractor(object):
    """Select uniformly distributed frames and extract its VGG feature."""

    def __init__(self, sess):
        """Load VGG model.

        Args:
            frame_num: number of frames per video.
            sess: tf.Session()
        """
        self.inputs = tf.placeholder(tf.float32, [1, 224, 224, 3])
        self.vgg16 = Vgg16()
        self.vgg16.build(self.inputs)
        self.sess = sess

    def extract_image(self, image):
        feature = self.sess.run(
            self.vgg16.relu7, feed_dict = {self.inputs: image})
        return feature