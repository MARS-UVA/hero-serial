# hero-serial

This repository contains the code base for the Hero microcontroller. It is responsible for direct motor control and reading from various sensors. 

Hero board's documentation: https://www.ctr-electronics.com/hro.html. Under the Tech Resources tab, you can find the Hero user guide and software documentation. 

## Development Prerequisite

You need to have the phenoix framework (available under the Tech Resources tab in the link above) and Visual Studio 2017 or newer (community edition is enough) installed.

## Serial Protocol Specification (NEW)

This protocol is a simple variable-length protocol. A valid packet consists of a sequence of bytes of the following format

```bytes
0xff count databytes checksum
```

0xff is the header byte

**count** is a single byte specifying the opcode of this message and the length of the data bytes. The two most significaint bits indicate the opcode. The lower 6 bits indicate the number of bytes in the databytes section. Alternatively, we can say that we have 10 header bits in total. Because the size is determined by 6 bits, the valid range for the length of the data bytes are between 0 and 63 (because we have 6 bits that we can use, and `2^6=64`). 

Opcode | Value | Description
---|---|---
00 | Stop | Stops and disables all motors
01 | Direct Control | Passes percent output values to each talon in order of their IDs
10 | PID Control | Given target values, the Hero will adjust the motors to meet those targets
11 | Reserved | Reserved for future use

**databytes** consist of arbitrary bytes whose length is specified by the lower 6 bits of **count** . These must be in the same order everytime. The order is as follows:
<ol>
	<li> Drive Front Left</li>
	<li> Drive Front Right</li>
	<li> Drive Back Left</li>
	<li> Drive Back Right</li>
	<li> Bucket Ladder</li>
	<li> Bucket Extension</li>
	<li> Bucket Chain Driver</li>
	<li> Basket Lifter</li>
</ol>

**checksum** is the sum of all bytes before it modulo 256. 

For example,

```bytes
0xff 0xc4 0x12 0x13 0x14 0x15 0x11
```

is a valid byte sequence for this protocol. Note that `0xc4 = 0b11000100 = 196`. The most significant 2 bits are set to 1, which indicates an opcode of 3 which is reserved. The next 6 represents that we have `0b100 = 4` data bytes. They are `0x12 0x13 0x14 0x15`. Finally, we find the sum of `0xff 0xc4 0x12 0x13 0x14 0x15 = 0x211`, and mod by `256 = 0x100`, and as the result we got `0x11` as the checksum. 

## Serial Protocol Specification (OLD)

This protocol is a simple variable-length protocol. A valid packet consists of a sequence of bytes of the following format

```bytes
0xff count databytes checksum
```

0xff is the header byte

**count** is a single byte specifying the length of the data bytes. We require the most significant 2 bits of the count byte to set to 1. Alternatively, we can say that we have 10 header bits in total. Because the 2 upper bits are set to 1, the valid range for the length of the data bytes are between 0 and 63 (because we have 6 bits that we can use, and `2^6=64`). 

**databytes** consist of arbitrary bytes whose length is specified by **count** (with the upper 2 bits set to 0, of course). 

**checksum** is the sum of all bytes before it modulo 256. 

For example,

```bytes
0xff 0xc4 0x12 0x13 0x14 0x15 0x11
```

is a valid byte sequence for this protocol. Note that `0xc4 = 0b11000100 = 196`. The most significant 2 bits are set to 1, so this byte represents that we have `0b100 = 4` data bytes. They are `0x12 0x13 0x14 0x15`. Finally, we find the sum of `0xff 0xc4 0x12 0x13 0x14 0x15 = 0x211`, and mod by `256 = 0x100`, and as the result we got `0x11` as the checksum. 
