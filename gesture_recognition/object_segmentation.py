# -*- coding: utf-8 -*-
# print(img.shape)
import cv2
import numpy as np
import matplotlib.pyplot as plt

def get_image(path):
	img = cv2.imread(path)
	print(path)
	gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
	return img, gray

# ??˹ȥ??
def Gaussian_Blur(gray):
	blurred = cv2.GaussianBlur(gray, (9, 9), 0)
	return blurred


def Sobel_gradient(blurred):
	gradX = cv2.Sobel(blurred, ddepth=cv2.CV_32F, dx=1, dy=0)
	gradY = cv2.Sobel(blurred, ddepth=cv2.CV_32F, dx=0, dy=1)
	gradient = cv2.subtract(gradX, gradY)
	gradient = cv2.convertScaleAbs(gradient)
	return gradX, gradY, gradient


def Thresh_and_blur(gradient):  # ?趨??ֵ
	blurred = cv2.GaussianBlur(gradient, (9, 9), 0)
	(_, thresh) = cv2.threshold(blurred, 20, 255, cv2.THRESH_BINARY)
	return thresh

#?˺???
def image_morphology(thresh):

	kernel = cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (25, 25))
	closed = cv2.morphologyEx(thresh, cv2.MORPH_CLOSE, kernel)
	closed = cv2.erode(closed, None, iterations=4)
	closed = cv2.dilate(closed, None, iterations=4)
	return closed


def findcnts_and_box_point(closed):
	(_, cnts, _) = cv2.findContours(closed.copy(),
	                                cv2.RETR_LIST,
	                                cv2.CHAIN_APPROX_SIMPLE)
	c = sorted(cnts, key=cv2.contourArea, reverse=True)[0]
	rect = cv2.minAreaRect(c)
	box = np.int0(cv2.boxPoints(rect))
	return box


def drawcnts_and_cut(original_img, box):  # Ŀ??ͼ??ü?
	# draw_img = cv2.drawContours(original_img.copy(), [box], -1, (0, 0, 255), 3)

	Xs = [i[0] for i in box]
	Ys = [i[1] for i in box]
	x1 = min(Xs)
	x2 = max(Xs)
	y1 = min(Ys)
	y2 = max(Ys)
	hight = y2 - y1
	width = x2 - x1
	crop_img = original_img[y1:y1 + hight, x1:x1 + width]
	return crop_img


def segmentation(img_path):
	# img_path = r'C:/Users/Setella/Desktop/gesture_recognition0727/data/gestures/1/2019_07_22_14_27_13.png'
	original_img, gray = get_image(img_path)
	blurred = Gaussian_Blur(gray)
	gradX, gradY, gradient = Sobel_gradient(blurred)
	thresh = Thresh_and_blur(gradient)
	closed = image_morphology(thresh)
	box = findcnts_and_box_point(closed)
	crop_img = drawcnts_and_cut(gray, box)

	# plt.imshow(crop_img)
	# plt.imshow(crop_img)
	# plt.show()
	return crop_img

def main():
	# segmentation('./data/gestures/1/2019_07_22_14_27_13.png')
    segmentation('C:/Users/Setella/Desktop/gesture_recognition0727/data/gestures/1/2019_07_22_14_27_13.png')
if __name__ == '__main__':
    main()
