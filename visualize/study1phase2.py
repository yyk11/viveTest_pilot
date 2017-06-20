from mpl_toolkits.mplot3d import Axes3D
import matplotlib.pyplot as plt
import numpy as np
import sys
import os

file = open("xwj2.txt","r")
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
for i in range(1,106):
	#print i
	numbers = map(float, lines[i].split(" ")[:3])
	#position_target.append(numbers)
	xs.append(numbers[0])
	ys.append(numbers[1])
	zs.append(numbers[2])
	numbers2 =map(float, lines[i+106].split(" ")[:3])
	#position_result.append(numbers)
	d = np.linalg.norm(np.array(numbers) - np.array(numbers2))
	v = np.array(numbers) - np.array(numbers2)
	vecs.append(v)
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
	vn = []
	for j in range(3):
		if v[j] >= 0:
			vn.append(1)
		else:
			vn.append(0) 
	vs.append(vn)
	if d > maxd:
		maxd =d
		print i
	if d < mind:
		mind = d
	distance.append(d)

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


for i in range(105):
	cs.append((vs[i][0],vs[i][1],vs[i][2]))
	ss.append((distance[i]-mind)/(maxd-mind)*100)
fig = plt.figure()
ax = fig.add_subplot(111, projection='3d')
ax.scatter(xs,ys,zs,c=cs,s=ss, marker = 'o')
#ax.scatter(xs,ys,zs,c=(1,0,0),s=ss, marker = 'o')

ax.set_xlabel('X Label')
ax.set_ylabel('Y Label')
ax.set_zlabel('Z Label')

plt.show()