import os
import sys

data_dir = "sit"

output_resolution = open(data_dir+"_resolution.txt","w")
output_comfortable = open(data_dir+"_comfortable.txt","w")

files = os.listdir(data_dir)
for file in files:
	input_data = open(data_dir+"/"+file,"r")
	lines = input_data.readlines()
	reso = ""
	comf = ""
	for line in lines:
		reso = reso + line.split(" ")[0] + " "
		comf = comf + line.split(" ")[1] + " "
	output_resolution.write(reso + "\n")
	output_comfortable.write(comf + "\n")
output_resolution.close()
output_comfortable.close()
