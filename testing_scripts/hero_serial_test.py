import serial   # pyserial
from pynput import keyboard # pynput
import threading
import time

forward = 0xC8  # 200 = 0xC8, 150 = 0x96, 125 = 7D
back = 0x00     # 0 = 0x00, 50 = 0x32, 75 = 0x4B
stop = 0x64     # 100 = 0x64

EXIT = False

# w = forward
# a = left
# s = reverse
# d = right
# p = stop
# x = stopcode

def on_press(key):
    try:
        if key.char == 'w':
            instruction = bytearray()
            instruction.append(0xFF)     # header, 255
            instruction.append(0x48)     # opcode + count: 01 001000, direct control, 8 data bytes, 72
            instruction.append(forward)     # front left, 150
            instruction.append(forward)     # front right, 150
            instruction.append(forward)     # back left, 150
            instruction.append(forward)     # back right, 150
            instruction.append(stop)     # bucket ladder angle, 100
            instruction.append(stop)     # bucket ladder translation, 100
            instruction.append(stop)     # bucket ladder chain driver, 100
            instruction.append(stop)     # deposit bin angle, 100
            checksum = (0xFF + 0x48 + (forward*4 + stop*4)) % 0x100
            instruction.append(checksum)     # checksum, 1327%256 = 47
            ser.write(instruction)
            print('forward')
        elif key.char =='a':
            instruction = bytearray()
            instruction.append(0xFF)     # header, 255
            instruction.append(0x48)     # opcode + count: 01 001000, direct control, 8 data bytes, 72
            instruction.append(back)     # front left, 50
            instruction.append(forward)     # front right, 150
            instruction.append(back)     # back left, 50
            instruction.append(forward)     # back right, 150
            instruction.append(stop)     # bucket ladder angle, 100
            instruction.append(stop)     # bucket ladder translation, 100
            instruction.append(stop)     # bucket ladder chain driver, 100
            instruction.append(stop)     # deposit bin angle, 100
            checksum = (0xFF + 0x48 + (forward*2 + back*2 + stop*4)) % 0x100
            instruction.append(checksum)     # checksum, 1127%256 = 103
            ser.write(instruction)
            print('left')
        elif key.char == 's':
            instruction = bytearray()
            instruction.append(0xFF)     # header, 255
            instruction.append(0x48)     # opcode + count: 01 001000, direct control, 8 data bytes, 72
            instruction.append(back)     # front left, 50
            instruction.append(back)     # front right, 50
            instruction.append(back)     # back left, 50
            instruction.append(back)     # back right, 50
            instruction.append(stop)     # bucket ladder angle, 100
            instruction.append(stop)     # bucket ladder translation, 100
            instruction.append(stop)     # bucket ladder chain driver, 100
            instruction.append(stop)     # deposit bin angle, 100
            checksum = (0xFF + 0x48 + (back*4 + stop*4)) % 0x100
            instruction.append(checksum)     # checksum, 927%256 = 159
            ser.write(instruction)
            print('back')
        elif key.char == 'd':
            instruction = bytearray()
            instruction.append(0xFF)     # header, 255
            instruction.append(0x48)     # opcode + count: 01 001000, direct control, 8 data bytes, 72
            instruction.append(forward)     # front left, 150
            instruction.append(back)     # front right, 50
            instruction.append(forward)     # back left, 150
            instruction.append(back)     # back right, 50
            instruction.append(stop)     # bucket ladder angle, 100
            instruction.append(stop)     # bucket ladder translation, 100
            instruction.append(stop)     # bucket ladder chain driver, 100
            instruction.append(stop)     # deposit bin angle, 100
            checksum = (0xFF + 0x48 + (forward*2 + back*2 + stop*4)) % 0x100
            instruction.append(checksum)     # checksum, 1127%256 = 103
            ser.write(instruction)
            print('right')
        elif key.char == 'x':           # STOP command
            instruction = bytearray()
            instruction.append(0xFF)    # header, 255
            instruction.append(0x00)    # opcode + count: 00 000000, stop, 0 databytes, 0
            checksum = (0xFF + 0x00) % 0x100
            instruction.append(checksum)    # checksum, 255%256 = 255
            ser.write(instruction)
            print('STOP')
        elif key.char == 'p':           # stop in the form of 0 driver
            instruction = bytearray()
            instruction.append(0xFF)     # header, 255
            instruction.append(0x48)     # opcode + count: 01 001000, direct control, 8 data bytes, 72
            instruction.append(stop)     # front left, 100
            instruction.append(stop)     # front right, 100
            instruction.append(stop)     # back left, 100
            instruction.append(stop)     # back right, 100
            instruction.append(stop)     # bucket ladder angle, 100
            instruction.append(stop)     # bucket ladder translation, 100
            instruction.append(stop)     # bucket ladder chain driver, 100
            instruction.append(stop)     # deposit bin angle, 100
            checksum = (0xFF + 0x48 + stop*8) % 0x100
            instruction.append(checksum)     # checksum, 1127%256 = 103
            ser.write(instruction)
            print('stop')
    except AttributeError:
       pass

def on_release(key):
    global EXIT
    if key == keyboard.Key.esc:
        EXIT = True;
        # stop listener upon releasing escape key
        return False

class input_thread(threading.Thread):
    def __init__(self):
        threading.Thread.__init__(self)
    def run(self):
        global EXIT
        with keyboard.Listener(on_press = on_press, on_release = on_release) as listener:
            listener.join()

class serial_thread(threading.Thread):
    def __init__(self):
        threading.Thread.__init__(self)
    def run(self):
        global EXIT
        # keep executing until keyboard listener stops and EXIT is true
        while not EXIT:
            while ser.inWaiting() > 0:
                # wait for header byte and print it
                in_b = ser.read(1)
                if (int.from_bytes(in_b, "big") == 0xFF):
                    print(in_b.hex())
                    # get next byte, print it, extract count, ignore opcode for now
                    in_b = ser.read(1)
                    print(in_b.hex())
                    count = int.from_bytes(in_b, "big") & 0x3F
                    # get data bytes, print them
                    for x in range(count):
                        in_b = ser.read(1)
                        print(in_b.hex())
                    # get checksum, print it, no verification for now
                    in_b = ser.read(1)
                    print(in_b.hex())
                    time.sleep(1)
                    ser.flushInput()

        # close serial port
        ser.close()

# START OF MAIN PROGRAM ------------------------------------------------------------------

# configure serial port
ser = serial.Serial(
    port = 'COM3',      # need to change per computer
    baudrate = 115200,
    parity = serial.PARITY_NONE,
    stopbits = serial.STOPBITS_ONE,
    bytesize = serial.EIGHTBITS
)

in_thread = input_thread()
ser_thread = serial_thread()

in_thread.start()
ser_thread.start()

in_thread.join()
ser_thread.join()

exit()
