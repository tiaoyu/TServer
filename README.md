TServer
=========
a game server combine anything I have learned.

### 0 To do list

| index | function | status |
| ----- | -------- | ------ |
| 1     | 断线重连 |        |
| 2     | 节点分离 |        |
| 3     |          |        |

### 1 基础网络层逻辑
* SocketAsyncEventArgs 池
* AsyncUserToken
* Buffer池
* ExtSocket 作为

### 2 消息处理逻辑

待续

### 3.功能
#### 3.1 协议定义

| C2S_Begin    | 0    | S2C_Begin    | 0    |
| ------------ | ---- | ------------ | ---- |
| C2S_Test     | 4999 | S2C_Test     | 5000 |
| C2S_Login    | 1    | S2C_Login    | 9999 |
| C2S_Register | 2    | S2C_Register | 9998 |
| C2S_Move     | 3    | S2C_Move     | 9997 |
| C2S_NavAuto  | 4    | S2C_Sight    | 9996 |
| C2S_StopMove | 5    | S2C_StopMove | 9995 |

### 4 视野管理逻辑（AOI）
#### AOI-格子
将地图分成二维格子阵列，每个格子具有相同固定大小，格子上存储当前格子内所有Entity
#### AOI-管理类
* 目标进入AOI
* 目标移出AOI
### 5 地图寻路逻辑

#### 5.1 构建3d地图模型 生成NavMesh

待续

#### 5.2 NavMesh寻路

待续

#### 简单A*寻路实现

#### 5.* 参考资料

[A Formal Basis for the Heuristic Determination of Minimum Cost Paths](https://www.cs.auckland.ac.nz/compsci767s2c/projectReportExamples.d/astarNilsson.pdf)
[A*-based Pathfinding in Modern Computer Games](https://www.researchgate.net/profile/Xiao_Cui7/publication/267809499_A-based_Pathfinding_in_Modern_Computer_Games/links/54fd73740cf270426d125adc.pdf)

### 6 角色战斗逻辑
### 7 房间匹配逻辑
### 8 队伍相关逻辑
### 9 副本相关逻辑
