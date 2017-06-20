from mpl_toolkits.mplot3d import Axes3D
import matplotlib.pyplot as plt
import numpy as np
import sys
import os

file = open("xwj.txt","r")
lines = file.readlines()
position_target = []
position_result = []
distance = []
vs = []
xs = []
ys = []
zs = []
cs = []
ss = []
maxd = 0
mind = 10000
for i in range(1,212):
	#print i
	if i == 106:
		continue
	numbers = map(float, lines[i].split(" ")[:3])
	#position_target.append(numbers)
	xs.append(numbers[0])
	ys.append(numbers[1])
	zs.append(numbers[2])






fig = plt.figure()
ax = fig.add_subplot(111, projection='3d')
ax.scatter(xs[:105],ys[:105],zs[:105],c='r', s=10,marker = 'o')
ax.scatter(xs[106:210],ys[106:210],zs[106:210],c='g', marker = 'o')
#ax.scatter(xs,ys,zs,c=(1,0,0),s=ss, marker = 'o')

ax.set_xlabel('X Label')
ax.set_ylabel('Y Label')
ax.set_zlabel('Z Label')

plt.show()