from mpl_toolkits.mplot3d import Axes3D
import matplotlib.pyplot as plt
import numpy as np
import sys
import os

file = open("1.txt", "r")
lines = file.readlines()
xs = []
ys = []
zs = []
for line in lines:
	numbers = map(float, line.split(" ")[:3])
	xs.append(numbers[0])
	ys.append(numbers[1])
	zs.append(numbers[2])


cs = ['b','r','y','g','k','b','r','y','g','k','b','r','y','g','k','b','r']
fig = plt.figure()
ax = fig.add_subplot(111, projection='3d')
ax.scatter(xs,ys,zs,c=cs, marker = 'o')
#ax.scatter(xs,ys,zs,c=(1,0,0),s=ss, marker = 'o')

ax.set_xlabel('X Label')
ax.set_ylabel('Y Label')
ax.set_zlabel('Z Label')

plt.show()