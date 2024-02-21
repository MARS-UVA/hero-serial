import serial   # pyserial
from pynput import keyboard # pynput
import threading
import time
import struct

#------------------------Controls-------------------------------------------#
    # P writes 0 to all motors
    # X sends stop opcode which will require a restart to get out of
    # W, A, S, D for drivetrain
    # R, F for bucket ladder angle
    # T, G for bucket ladder translation
    # Y, H for bucket ladder chain driver
    # U, J for conveyor driver
    # I, K for deposit bin
    # O, L for IR sensor
#---------------------------------------------------------------------------#

#-----------------------Protocol Bytes--------------------------------------#
header = 0xFF
direct_opcount = 0x49                   # 01 001001, direct control, 9 data bytes
stop_opcount = 0x00

def calculate_checksum(instruction):
    checksum = 0
    for val in instruction:
        checksum += val
    return checksum % 0x100
#---------------------------------------------------------------------------#

#-----------------------Stop Motor Value------------------------------------#
stop = 100
#---------------------------------------------------------------------------#

#-----------------------Drivetrain Motor Values-----------------------------#
dt_forw = 150
dt_back = 50
#---------------------------------------------------------------------------#

#-----------------------Deposit Motor Values----------------------------#
db_up = 150
db_down = 50
conv_forw = 180
conv_rev = 20
#---------------------------------------------------------------------------#

#-----------------------Bucket Ladder Motor Values----------------------------#
bl_up = 150
bl_down = 50
bl_out = 150
bl_in = 50
bl_chain_forw = 150
bl_chain_rev = 50
#---------------------------------------------------------------------------#

#-----------------------IR Sensor Motor Values----------------------------#
ir_opcode = 48 # 01 001000, direct control, 8 data bytes
# 01 001001, direct control, 9 data bytes
ir_data = [0xF12A1674, 0x7A8F3D2E, 0xD4E7B9A2, 0x2F6D8A3C]
angles = [0x65167315, 0x9B0C5A1F, 0x3E7A8F3D, 0x2E6D8A3C]
#---------------------------------------------------------------------------#
interpret_data_as_floats = True;

EXIT = False

def on_press(key):
    try:
        if key.char == 'w':
            instruction = bytearray()
            instruction.append(header)     # header, 255
            instruction.append(direct_opcount)     # opcode + count: 01 001000, direct control, 8 data bytes, 72
            instruction.append(dt_forw)     # front left, 150
            instruction.append(dt_forw)     # front right, 150
            instruction.append(dt_forw)     # back left, 150
            instruction.append(dt_forw)     # back right, 150
            instruction.append(stop)     # bucket ladder angle, 100
            instruction.append(stop)     # bucket ladder translation, 100
            instruction.append(stop)     # bucket ladder chain driver, 100
            instruction.append(stop)     # deposit bin angle, 100
            instruction.append(stop)     # conveyor driver
            instruction.append(calculate_checksum(instruction))     # checksum, 1327%256 = 47
            ser.write(instruction)
            print('drive forward')
        elif key.char =='a':
            instruction = bytearray()
            instruction.append(header)     # header, 255
            instruction.append(direct_opcount)     # opcode + count: 01 001000, direct control, 8 data bytes, 72
            instruction.append(dt_back)     # front left, 50
            instruction.append(dt_forw)     # front right, 150
            instruction.append(dt_back)     # back left, 50
            instruction.append(dt_forw)     # back right, 150
            instruction.append(stop)     # bucket ladder angle, 100
            instruction.append(stop)     # bucket ladder translation, 100
            instruction.append(stop)     # bucket ladder chain driver, 100
            instruction.append(stop)     # deposit bin angle, 100
            instruction.append(stop)     # conveyor driver
            instruction.append(calculate_checksum(instruction))     # checksum, 1127%256 = 103
            ser.write(instruction)
            print('drive left')
        elif key.char == 's':
            instruction = bytearray()
            instruction.append(header)     # header, 255
            instruction.append(direct_opcount)     # opcode + count: 01 001000, direct control, 8 data bytes, 72
            instruction.append(dt_back)     # front left, 50
            instruction.append(dt_back)     # front right, 50
            instruction.append(dt_back)     # back left, 50
            instruction.append(dt_back)     # back right, 50
            instruction.append(stop)     # bucket ladder angle, 100
            instruction.append(stop)     # bucket ladder translation, 100
            instruction.append(stop)     # bucket ladder chain driver, 100
            instruction.append(stop)     # deposit bin angle, 100
            instruction.append(stop)     # conveyor driver
            instruction.append(calculate_checksum(instruction))     # checksum, 927%256 = 159
            ser.write(instruction)
            print('drive back')
        elif key.char == 'd':
            instruction = bytearray()
            instruction.append(header)     # header, 255
            instruction.append(direct_opcount)     # opcode + count: 01 001000, direct control, 8 data bytes, 72
            instruction.append(dt_forw)     # front left, 150
            instruction.append(dt_back)     # front right, 50
            instruction.append(dt_forw)     # back left, 150
            instruction.append(dt_back)     # back right, 50
            instruction.append(stop)     # bucket ladder angle, 100
            instruction.append(stop)     # bucket ladder translation, 100
            instruction.append(stop)     # bucket ladder chain driver, 100
            instruction.append(stop)     # deposit bin angle, 100
            instruction.append(stop)     # conveyor driver
            instruction.append(calculate_checksum(instruction))     # checksum, 1127%256 = 103
            ser.write(instruction)
            print('drive right')
        elif key.char == 'r':
            instruction = bytearray()
            instruction.append(header)
            instruction.append(direct_opcount)
            instruction.append(stop) # front left drive
            instruction.append(stop) # front right drive
            instruction.append(stop) # back left drive
            instruction.append(stop) # back right drive
            instruction.append(bl_up) # bucket ladder angle
            instruction.append(stop) # bucket ladder translation
            instruction.append(stop) # bucket ladder chain driver
            instruction.append(stop) # deposit bin
            instruction.append(stop) # conveyor driver
            instruction.append(calculate_checksum(instruction))
            ser.write(instruction)
            print('bucket ladder up')
        elif key.char == 'f':
            instruction = bytearray()
            instruction.append(header)
            instruction.append(direct_opcount)
            instruction.append(stop) # front left drive
            instruction.append(stop) # front right drive
            instruction.append(stop) # back left drive
            instruction.append(stop) # back right drive
            instruction.append(bl_down) # bucket ladder angle
            instruction.append(stop) # bucket ladder translation
            instruction.append(stop) # bucket ladder chain driver
            instruction.append(stop) # deposit bin
            instruction.append(stop) # conveyor driver
            instruction.append(calculate_checksum(instruction))
            ser.write(instruction)
            print('bucket ladder down')
        elif key.char == 't':
            instruction = bytearray()
            instruction.append(header)
            instruction.append(direct_opcount)
            instruction.append(stop) # front left drive
            instruction.append(stop) # front right drive
            instruction.append(stop) # back left drive
            instruction.append(stop) # back right drive
            instruction.append(stop) # bucket ladder angle
            instruction.append(bl_out) # bucket ladder translation
            instruction.append(stop) # bucket ladder chain driver
            instruction.append(stop) # deposit bin
            instruction.append(stop) # conveyor driver
            instruction.append(calculate_checksum(instruction))
            ser.write(instruction)
            print('bucket ladder out')
        elif key.char == 'g':
            instruction = bytearray()
            instruction.append(header)
            instruction.append(direct_opcount)
            instruction.append(stop) # front left drive
            instruction.append(stop) # front right drive
            instruction.append(stop) # back left drive
            instruction.append(stop) # back right drive
            instruction.append(stop) # bucket ladder angle
            instruction.append(bl_in) # bucket ladder translation
            instruction.append(stop) # bucket ladder chain driver
            instruction.append(stop) # deposit bin
            instruction.append(stop) # conveyor driver
            instruction.append(calculate_checksum(instruction))
            ser.write(instruction)
            print('bucket ladder in')
        elif key.char == 'y':
            instruction = bytearray()
            instruction.append(header)
            instruction.append(direct_opcount)
            instruction.append(stop) # front left drive
            instruction.append(stop) # front right drive
            instruction.append(stop) # back left drive
            instruction.append(stop) # back right drive
            instruction.append(stop) # bucket ladder angle
            instruction.append(stop) # bucket ladder translation
            instruction.append(bl_chain_forw) # bucket ladder chain driver
            instruction.append(stop) # deposit bin
            instruction.append(stop) # conveyor driver
            instruction.append(calculate_checksum(instruction))
            ser.write(instruction)
            print('bucket ladder chain forward')
        elif key.char == 'h':
            instruction = bytearray()
            instruction.append(header)
            instruction.append(direct_opcount)
            instruction.append(stop) # front left drive
            instruction.append(stop) # front right drive
            instruction.append(stop) # back left drive
            instruction.append(stop) # back right drive
            instruction.append(stop) # bucket ladder angle
            instruction.append(stop) # bucket ladder translation
            instruction.append(bl_chain_rev) # bucket ladder chain driver
            instruction.append(stop) # deposit bin
            instruction.append(stop) # conveyor driver
            instruction.append(calculate_checksum(instruction))
            ser.write(instruction)
            print('bucket ladder chain reverse')
        elif key.char == 'u':
            instruction = bytearray()
            instruction.append(header)
            instruction.append(direct_opcount)
            instruction.append(stop) # front left drive
            instruction.append(stop) # front right drive
            instruction.append(stop) # back left drive
            instruction.append(stop) # back right drive
            instruction.append(stop) # bucket ladder angle
            instruction.append(stop) # bucket ladder translation
            instruction.append(stop) # bucket ladder chain driver
            instruction.append(stop) # deposit bin
            instruction.append(conv_forw) # conveyor driver
            instruction.append(calculate_checksum(instruction))
            ser.write(instruction)
            print('conveyor forward')
        elif key.char == 'j':
            instruction = bytearray()
            instruction.append(header)
            instruction.append(direct_opcount)
            instruction.append(stop) # front left drive
            instruction.append(stop) # front right drive
            instruction.append(stop) # back left drive
            instruction.append(stop) # back right drive
            instruction.append(stop) # bucket ladder angle
            instruction.append(stop) # bucket ladder translation
            instruction.append(stop) # bucket ladder chain driver
            instruction.append(stop) # deposit bin
            instruction.append(conv_rev) # conveyor driver
            instruction.append(calculate_checksum(instruction))
            ser.write(instruction)
            print('conveyor reverse')
        elif key.char == 'i':
            instruction = bytearray()
            instruction.append(header)
            instruction.append(direct_opcount)
            instruction.append(stop) # front left drive
            instruction.append(stop) # front right drive
            instruction.append(stop) # back left drive
            instruction.append(stop) # back right drive
            instruction.append(stop) # bucket ladder angle
            instruction.append(stop) # bucket ladder translation
            instruction.append(stop) # bucket ladder chain driver
            instruction.append(db_up) # deposit bin
            instruction.append(stop) # conveyor driver
            instruction.append(calculate_checksum(instruction))
            ser.write(instruction)
            print('deposit bin up')
        elif key.char == 'k':
            instruction = bytearray()
            instruction.append(header)
            instruction.append(direct_opcount)
            instruction.append(stop) # front left drive
            instruction.append(stop) # front right drive
            instruction.append(stop) # back left drive
            instruction.append(stop) # back right drive
            instruction.append(stop) # bucket ladder angle
            instruction.append(stop) # bucket ladder translation
            instruction.append(stop) # bucket ladder chain driver
            instruction.append(db_down) # deposit bin
            instruction.append(stop) # conveyor driver
            instruction.append(calculate_checksum(instruction))
            ser.write(instruction)
            print('deposit bin down')
        elif key.char == 'x':           # STOP command
            instruction = bytearray()
            instruction.append(header)    # header, 255
            instruction.append(stop_opcount)    # opcode + count: 00 000000, stop, 0 databytes, 0
            instruction.append(calculate_checksum(instruction))    # checksum, 255%256 = 255
            ser.write(instruction)
            print('STOP')
        elif key.char == 'p':           # stop in the form of 0 driver
            instruction = bytearray()
            instruction.append(header)     # header, 255
            instruction.append(direct_opcount)     # opcode + count: 01 001000, direct control, 8 data bytes, 72
            instruction.append(stop)     # front left, 100
            instruction.append(stop)     # front right, 100
            instruction.append(stop)     # back left, 100
            instruction.append(stop)     # back right, 100
            instruction.append(stop)     # bucket ladder angle, 100
            instruction.append(stop)     # bucket ladder translation, 100
            instruction.append(stop)     # bucket ladder chain driver, 100
            instruction.append(stop)     # deposit bin angle, 100
            instruction.append(stop)     # conveyor driver
            instruction.append(calculate_checksum(instruction))     # checksum, 1127%256 = 103
            ser.write(instruction)
            print('stop')
        elif key.char == 'o':
            instruction = bytearray()
            instruction.append(header)     # header, 255
            instruction.append(direct_opcount)     # opcode + count: 01 001000, direct control, 8 data bytes, 72
            
            instruction.append(calculate_checksum(instruction))     # checksum, 927%256 = 159
            ser.write(instruction)
            print('ir sensor')
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
                    float_bytes = bytearray()
                    float_count = 0
                    for x in range(count):
                        in_b = ser.read(1)
                        if interpret_data_as_floats:
                            float_bytes += in_b
                            float_count += 1
                            if (float_count == 4):
                                print(struct.unpack('f', float_bytes))
                                float_count = 0
                                float_bytes = bytearray()
                        else:
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
