import serial   # pyserial
from pynput import keyboard # pynput
import time

forward = 0x96  # 150
back = 0x32     # 50
stop = 0x64     # 100

# w = forward
# a = left
# s = reverse
# d = right
# x = stop

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
            instruction.append(0x2F)     # checksum, 1327%256 = 47
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
            instruction.append(0x67)     # checksum, 1127%256 = 103
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
            instruction.append(0x9F)     # checksum, 927%256 = 159
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
            instruction.append(0x67)     # checksum, 1127%256 = 103
            ser.write(instruction)
            print('right')
        elif key.char == 'x':           # STOP command
            instruction = bytearray()
            instruction.append(0xFF)    # header, 255
            instruction.append(0x00)    # opcode + count: 00 000000, stop, 0 databytes, 0
            instruction.append(0xFF)    # checksum, 255%256 = 255
            ser.write(instruction)
            print('STOP')
        elif key.char == 'p':           # stop in the form of 0 driver
            instruction = bytearray()
            instruction.append(0xFF)     # header, 255
            instruction.append(0x48)     # opcode + count: 01 001000, direct control, 8 data bytes, 72
            instruction.append(0x64)     # front left, 100
            instruction.append(0x64)     # front right, 100
            instruction.append(0x64)     # back left, 100
            instruction.append(0x64)     # back right, 100
            instruction.append(0x64)     # bucket ladder angle, 100
            instruction.append(0x64)     # bucket ladder translation, 100
            instruction.append(0x64)     # bucket ladder chain driver, 100
            instruction.append(0x64)     # deposit bin angle, 100
            instruction.append(0x67)     # checksum, 1127%256 = 103
            ser.write(instruction)
            print('stop')
    except AttributeError:
       pass

def on_release(key):
    # try:
    #     if key.char == 'w' or key.char == 'a' or key.char == 's' or key.char == 'd':
    #         instruction = bytearray()
    #         instruction.append(0xFF)     # header, 255
    #         instruction.append(0x48)     # opcode + count: 01 001000, direct control, 8 data bytes, 72
    #         instruction.append(0x64)     # front right, 100
    #         instruction.append(0x64)     # back right, 100
    #         instruction.append(0x64)     # front left, 100
    #         instruction.append(0x64)     # back left, 100
    #         instruction.append(0x64)     # bucket ladder angle, 100
    #         instruction.append(0x64)     # bucket ladder translation, 100
    #         instruction.append(0x64)     # bucket ladder chain driver, 100
    #         instruction.append(0x64)     # deposit bin angle, 100
    #         instruction.append(0x67)     # checksum, 1127%256 = 103
    #         ser.write(instruction)
    #         print('stop')
    # except AttributeError:
    if key == keyboard.Key.esc:
        # stop listener upon releasing escape key
        return False

# configure serial port
ser = serial.Serial(
    port = 'COM4',      # may need to change per computer
    baudrate = 115200,
    parity = serial.PARITY_NONE,
    stopbits = serial.STOPBITS_ONE,
    bytesize = serial.EIGHTBITS
)

# start keyboard listener until released
with keyboard.Listener(on_press = on_press, on_release = on_release) as listener:
    listener.join()

# close serial port
ser.close()
exit()
