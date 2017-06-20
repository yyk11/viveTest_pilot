import os
import sys
import random
import shutil

def findfiles(dirname):
	names = os.listdir(dirname);
	for name in names:
		if os.path.isdir(dirname+'/'+name):
			findfiles(dirname+'/'+name);
		elif os.path.isfile(dirname+'/'+name):
			parts = name.split('.')
			if parts[len(parts)-1] != 'meta':
				#outfile.write('"' + name.split('.')[0] + '",');
				filenames.append(name.split('.')[0])
				fullnames.append(dirname+'/'+name)
				

directory = 'Prefabs'
outdir = 'Select/'

outfile = open('name_prefab.txt','w')

filenames = []
fullnames = []

findfiles(directory);

filenames = random.sample(filenames, 180);

for i in range(len(filenames)):
	outfile.write('"' + filenames[i].split('.')[0] + '",')
	shutil.copyfile(fullnames[i], outdir+filenames[i]+'.prefab')
outfile.close()
