TServer
=========
a game server combine anything I have learned.

### 0 To do list

| index | function | status |
| ----- | -------- | ------ |
|       |          |        |

### 1 基础网络层逻辑
### 2 消息处理逻辑
#### 2.1 协议定义

|              |      |
| ------------ | ---- |
| C2S_Begin    | 0    |
| ------------ | ---- |
| C2S_Test     | 4999 |
| C2S_Login    | 1    |
| C2S_Register | 2    |
| C2S_Move     | 3    |
| C2S_NavAuto  | 4     |

|              |      |
| ------------ | ---- |
| S2C_Begin    | 0    |
| S2C_Test     | 5000 |
| S2C_Login    | 9999 |
| S2C_Register | 9998 |
| S2C_Move     | 9997 |
| S2C_Sight    | 9996 |

### 3 视野管理逻辑（AOI）
### 4 地图寻路逻辑

#### 4.1 构建3d地图模型 生成NavMesh

待续

#### 4.2 NavMesh寻路

待续

#### 4.* 参考资料

[A Formal Basis for the Heuristic Determination of Minimum Cost Paths](https://www.cs.auckland.ac.nz/compsci767s2c/projectReportExamples.d/astarNilsson.pdf)
[A*-based Pathfinding in Modern Computer Games](https://www.researchgate.net/profile/Xiao_Cui7/publication/267809499_A-based_Pathfinding_in_Modern_Computer_Games/links/54fd73740cf270426d125adc.pdf)

### 5 角色战斗逻辑
### 6 房间匹配逻辑
### 7 队伍相关逻辑
