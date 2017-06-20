import os
import httplib
import sys
import time

import urllib2


try:
    
    while True:
    	#print "here"
        #print response
        content = urllib2.urlopen("http://127.0.0.1:8000/1?1?1?1?1?1?1").read()
        print content
        time.sleep(0.1);
except Exception, e: 
    print "here";
    print e
