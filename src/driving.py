import random
import serial
import socket
import time

# Socket port information
HOST = "10.138.226.85"  # Standard loopback interface address (localhost)
PORT = 65321  # Port to listen on (non-privileged ports are > 1023)

# Serial port configuration
ser = serial.Serial(
    port="/dev/ttyUSB0",  # Replace ttyS0 with ttyAM0 for Pi1,Pi2,Pi0
    baudrate=115200,
    timeout=1,
)

# Socket configuration
s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
# Allow reconect when the server falls
s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
# Bind port
s.bind((HOST, PORT))
s.listen()
conn, addr = s.accept()
with conn:
    print("Connected by", addr)
    while True:
        data = conn.recv(1024)
        if not data:
            # Wait for a new connection if the socket stops transmitting
            print("Waiting for connection")
            s.close()
            s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
            s.bind((HOST, PORT))
            s.listen()
            conn, addr = s.accept()
        else:
            print(data)
            ser.write(data.decode('utf-8').encode())