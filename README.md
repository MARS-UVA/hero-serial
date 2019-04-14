# hero-serial

## Protocol Specification

A valid packet consists of a sequence of bytes of the following format

```bytes
0xff 0xff b1 b2 b3 b4 checksum
```

There are seven bytes in total.

0xff 0xff are the two header bytes required.

b1, b2, b3 and b4 are the four bytes specifying the motor percent output. An offset of +127 by default is added. Therefore, -100%  output should be sent as 27 (0x1b), and 100% output should be sent as 227 (0xe3)

checksum is a single bytes that is the sum of first six bytes (from headers to b4 inclusive) modulo 255. 
