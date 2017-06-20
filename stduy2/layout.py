from mpl_toolkits.mplot3d import Axes3D
import matplotlib.pyplot as plt
import numpy as np
import sys
import os

xs=[]
ys=[]
zs=[]
cs=[]
c = ['r','y','g','b']
num = 2

dirs = os.listdir(".")
for d in dirs:
	if os.path.isdir(d):
		files = os.listdir(d);
		file = open(d+"/"+files[num])
		lines = file.readlines()
		print d
		print len(lines)
		for i in range(len(lines)):
			numbers = lines[i].split(' ')
			xs.append(float(numbers[0]))
			ys.append(float(numbers[1]))
			zs.append(float(numbers[2]))
			cs.append(c[i%len(c)])

fig = plt.figure()
ax = fig.add_subplot(111, projection='3d')
ax.scatter(xs,ys,zs,c=cs, marker = 'o')
#ax.scatter(xs,ys,zs,c=(1,0,0),s=ss, marker = 'o')

ax.set_xlabel('X Label')
ax.set_ylabel('Y Label')
ax.set_zlabel('Z Label')

plt.show()