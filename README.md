# memory-cache
Memory cache sever application

## Structure
- MemoryCache.Parser - contains zero-allocation parser commands from array characters
- MemoryCache.Storage - cache storage
- MemoryCache.Tcp - TCP Server
- MemoryCache.Tests - unit tests

## Run
1. Run project - TestConsole
2. Open Terminal application
3. Enter command: `nc 127.0.0.1 8080`
4. After success connect to TCP server, enter command, for example, "`SET key1 data`"