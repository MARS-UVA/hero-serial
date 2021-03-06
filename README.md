# hero-serial

This repository contains the code base for the hero microcontroller. It is responsible for direct motor control and reading from various sensors. 

## Serial Protocol Specification

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