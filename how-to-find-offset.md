# 如何查找适用于当前版本游戏客户端的偏移地址
### 原理

渔人的直感根据客户端EventPlay事件所执行的动作来判断玩家的当前状态。例如抛竿动作、鱼轻杆咬钩动作、脱钩动作等。
在客户端接收到EventPlay的数据包后会对其中所指定的动作进行即时处理。
其在内存中所使用的临时变量结构特征非常不明显，难以通过AOB Scan等方法自动获取。

万幸的是，这个临时变量的地址在每次游戏客户端更新后是固定不变的，
因此我们可以借助内存查找工具（例如常见的Cheat Engine）来找到它并将其填入直感的配置文件中，这样直感就可以在版本更新后正常工作了。

### 查找方法
<img width="600" src="https://github.com/Natsukage/Assets/blob/main/FishersIntuition/images/CE1.png"/>  
打开Cheat Engine，加载最终幻想14的游戏进程，  
将搜索区域限定为ffxiv_dx11.exe内，搜索类型限定为2 Bytes长度的特定值，并确认勾选左侧的`Hex`选项，即搜索十六进制数值。
  
-  下文中所提到的ID均为十六进制形式表述。

<img width="600" src="https://github.com/Natsukage/Assets/blob/main/FishersIntuition/images/CE2.png"/>
确定你当前使用的是通常鱼饵（例如海水、淡水万能饵）后，站立抛竿，在鱼咬钩之前，在内存中搜索值 `112`（站立抛竿动作的ID）。
找到结果后，在游戏中输入`/sit`或`/坐下`指令或执行坐下情感动作，切换为坐下状态后再次抛竿，并在鱼咬钩前
搜索值 `C49`（坐下抛竿动作的ID）。反复数次后即可得到唯一地址（通常只需要1-2次）。
  
-  如果你直接坐下抛竿并从`C49`开始搜索，你有很大概率直接命中目标！

<img width="600" src="https://github.com/Natsukage/Assets/blob/main/FishersIntuition/images/CE3.png"/>

右键左侧的这个地址，选择Copy selected addresses后在任意位置粘贴即可得到形如`ffxiv_dx11.exe+1CAB970`的地址，
其中的加号后部分`1CAB970`即为我们需要的偏移数值。
将这个数值填入直感右键设置菜单中的文本框中，点击`应用`并重启直感后即可正常工作。
