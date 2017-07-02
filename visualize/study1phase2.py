from mpl_toolkits.mplot3d import Axes3D
import matplotlib.pyplot as plt
import numpy as np
import sys
import os
import random

file = open("yyk_study2_2.txt","r")
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
vecs = []
maxd = 0
mind = 10000
color = [0,0,0]

for i in range(1,721):
	#print i
	numbers = map(float, lines[i].split(" ")[:3])
	numbers2 =map(float, lines[i+721].split(" ")[:3])
	#position_target.append(numbers)
	xs.append(numbers2[0])
	ys.append(numbers2[1])
	zs.append(numbers2[2])
	xs.append(numbers[0])
	ys.append(numbers[1])
	zs.append(numbers[2])
	if (i % 12) == 1:
		#print i
		color[0] = random.random()
		color[1] = random.random()
		color[2] = random.random()
		#print color
	c = []
	c.append(color[0])
	c.append(color[1])
	c.append(color[2])
	vecs.append(c)
	vecs.append(c)
	#position_result.append(numbers)
	d = np.linalg.norm(np.array(numbers) - np.array(numbers2))
	#v = np.array(numbers) - np.array(numbers2)
	
	#vecs.append(v)
	#max direction
	#maxv = 0
	#if abs(v[1]) > abs(v[maxv]):
	#	maxv = 1
	#if abs(v[2]) > abs(v[maxv]):
	#	maxv = 2
	#vn = [0,0,0]
	#vn[maxv] = 1
	#vs.append(vn)

	# three direction
	if d > maxd:
		maxd =d
		print i
	if d < mind:
		mind = d
	distance.append(d)
	distance.append(1)




for i in range(720*2):
	#print vecs[i]
	cs.append((vecs[i][0],vecs[i][1],vecs[i][2]))
	ss.append((distance[i]-mind)/(maxd-mind)*100)
fig = plt.figure()
ax = fig.add_subplot(111, projection='3d')
ax.scatter(xs,ys,zs,c=cs,s=ss, marker = 'o')
#ax.scatter(xs,ys,zs,c=(1,0,0),s=ss, marker = 'o')

ax.set_xlabel('X Label')
ax.set_ylabel('Y Label')
ax.set_zlabel('Z Label')

plt.show()