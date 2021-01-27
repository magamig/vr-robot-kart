#!/bin/bash
# replace host=10.138.226.69 with correct IP address 
raspivid -n -t 0 -rot 270 -w 640 -h 480 -fps 20 -b 2500000 -vs -drc high -cs 0 -o - | gst-launch-1.0 -e -vvvv fdsrc ! h264parse ! rtph264pay pt=96 config-interval=1 ! udpsink host=10.138.226.69 port=7002