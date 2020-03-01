TServer
=========
a game server

### 0 To do list

| index | task                   |          |
| ----- | ---------------------- | -------- |
| 1     | 角色进入副本、退出副本 | FINISHED |
| 2     | 通知系统               |          |
| 3     | 匹配功能               |          |
| 4     |                        |          |
| 5     |                        |          |
| 6     |                        |          |
| 7     |                        |          |
| 8     |                        |          |
| 9     |                        |          |
| 10    |                        |          |

### 1 Function List

1. Socket list
2. Buffer manager
3. Role manager
4. Dungeon manager

### System

#### AOI

#### Login

### Protocol

|              |      |
| ------------ | ---- |
| C2S_Begin    | 0    |
| ------------ | ---- |
| C2S_Test     | 4999 |
| C2S_Login    | 1    |
| C2S_Register | 2    |
| C2S_Move     | 3    |
|              |      |

|              |      |
| ------------ | ---- |
| S2C_Begin    | 0    |
| S2C_Test     | 5000 |
| S2C_Login    | 9999 |
| S2C_Register | 9998 |
| S2C_Move     | 9997 |
| S2C_Sight    | 9996 |
