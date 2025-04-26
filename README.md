# EvilIP

EvilIP is a simple client-server application that leverages the "evil bit" and utilizes ICMP (Internet Control Message Protocol) to mask data transmission.
This project serves as a proof-of-concept for covert communication channels using network protocols in unconventional ways.​

## Overview

The application comprises three main components:​

- EvilClient: Sends data packets with the evil bit set, encapsulating the payload within ICMP messages.
- EvilServer: Listens for incoming ICMP packets with the evil bit set and extracts the hidden payload.
- EvilPacket: Defines the structure and handling of the custom packets used for communication.​

By exploiting the lesser-used "evil bit" in the IP header and embedding data within ICMP packets, EvilIP demonstrates how data can be transmitted covertly across networks.​

## Features

- Utilizes the IP header's evil bit for packet identification.
- Encapsulates data within ICMP packets to mask the payload.
- Simple client-server architecture for sending and receiving data.
- Written entirely in C#.​

## Prerequisites

- .NET Framework or .NET Core SDK installed on your system.
- Administrator or root privileges may be required to send and receive raw ICMP packets.​

# Building the Project

1. Clone the repository:​
```cmd
git clone https://github.com/jabez007/evilip.git
```
2. Navigate to the project directory:​
```cmd
cd evilip
```
3. Open the EvilIP.sln solution file in Visual Studio or your preferred C# IDE.​
4. Build the solution to compile the EvilClient and EvilServer executables.​

## Usage

1. Start the Server: Run the EvilServer application to begin listening for incoming ICMP packets with the evil bit set.
2. Send Data with the Client: Use the EvilClient application to send data. The client will encapsulate the payload within an ICMP packet, set the evil bit, and transmit it to the server.

Ensure that any firewalls or network security settings allow for the transmission and reception of ICMP packets.​

## Disclaimer

This project is intended for educational and research purposes only.
Unauthorized use of this software to intercept, send, or receive data on networks without permission is illegal and unethical.
The author assumes no responsibility for any misuse of this application.
