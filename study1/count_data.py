import os
import sys

file1 = open("stand_resolution.txt","r")
file2 = open("stand_comfortable.txt", "r")

numbers_reso = []
ave_reso = []
lines = file1.readlines()
for line in lines:
	line = line[0:len(line)-2]
	numbers = map(float,line.split(" "))
	numbers_reso.append(numbers)

for i in range(len(numbers_reso[0])):
	ave_reso.append(0)
for numbers in numbers_reso:
	for i in range(len(numbers)):
		ave_reso[i] += numbers[i]

for i in range(len(numbers_reso[0])):
	ave_reso[i] /= len(numbers_reso)

height = []
width = []
for i in range(5):
	height.append(0)
for j in range(12):
	width.append(0)
for i in range(5):
	for j in range(12):
		height[i] += ave_reso[i*12+j]/12
		width[j] += ave_reso[i*12+j]/5

print height
print width


lines = file2.readlines()
for line in lines:
	line = line[0:len(line)-2]
	numbers = map(int,line.split(" "))
	numbers_reso.append(numbers)

for i in range(len(numbers_reso[0])):
	ave_reso.append(0)
for numbers in numbers_reso:
	for i in range(len(numbers)):
		ave_reso[i] += numbers[i]

for i in range(len(numbers_reso[0])):
	ave_reso[i] /= len(numbers_reso)

height = []
width = []
for i in range(5):
	height.append(0)
for j in range(12):
	width.append(0)
for i in range(5):
	for j in range(12):
		height[i] += ave_reso[i*12+j]/12
		width[j] += ave_reso[i*12+j]/5

print height
print width