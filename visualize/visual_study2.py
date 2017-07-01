from mpl_toolkits.mplot3d import Axes3D
import matplotlib.pyplot as plt
import numpy as np
import sys
import os

print "height"
for i in range(5):
	ave = 0
	vave = [0.,0.,0.]
	vave = np.array(vave)
	for j in range(105/5):
		ave += distance[i*21+j]
		vave += vecs[i*21+j]
	print ave/21
	#print vave/36

print "distance"
dis = [0,0,0]
for j in range(3):
	vave = [0.,0.,0.]
	vave = np.array(vave)
	for i in range(5):
		for k in range(7):
			dis[j] += distance[i*21+k+j*7]
			vave += vecs[i*21+k+j*7]
	print dis[j]/35
	#print vave/60

print "angle"
for k in range(7):
	angle = 0
	vave = [0.,0.,0.]
	vave = np.array(vave)
	for i in range(3):
		for j in range(5):
			angle += distance[i*21+j*7+k]
			vave += vecs[i*21+k+j*7]
	print angle/15
	#print vave/15