# 渔人的直感 

<img width="400" src="https://raw.githubusercontent.com/Natsukage/Assets/main/FishersIntuition/images/FI1.jpg"/>
渔人的直感是一个最终幻想14适用的简单的钓鱼计时器。
适用于国服与国际服客户端。仅支持64位DX11客户端，需要使用管理员权限启动。

## 主要特性
- 支持国服与国际服最新版本游戏客户端。
- 支持幻海流与空岛特殊天气计时。当特殊天气发生时将会自动开始进行倒计时。
- 10秒内短杆计时条速度为通常的3倍，方便在幻海流等情况下提升不同鱼种区分度。
- 支持自定义界面长度与宽度。
- 支持自定义计时条颜色与不同杆种咬钩时显示的颜色。
- 支持自定义咬钩时播放的音效（也可以不使用提示声音）。
- 可以设置令窗体在任务栏与Alt+Tab切换页中不显示。
- 可以设置令窗体在不活动时隐藏。

#### 1.38测试版更新内容
- **测试特性：坐标偏移改为自动获取，支持国服与国际服最新版客户端。**
- 增加了托盘图标，可以右键呼出设置与退出
- 窗体变为默认从任务栏与Alt+Tab中隐藏
- 窗体支持鼠标穿透
- 重置按钮将会同时重置直感的窗体位置
- 空岛特殊天气计时可以选择开关

感谢[@PrototypeSeiren](https://github.com/PrototypeSeiren)大佬的技术支持!

## 使用方法

启动渔人的直感后，软件将会处于非常不明显的半透明的隐藏状态，可以进行拖动。
右键点击计时条可以开启设置或跳转钓鱼时钟。

### 抛竿计时

在抛竿后，计时条将会开始进行计时。
当有鱼咬钩时，将会停止计时并在计时条上提示咬钩杆种。

<details>
<summary>咬钩时的提示样式</summary>

<img width="400" src="https://raw.githubusercontent.com/Natsukage/Assets/main/FishersIntuition/images/FI2.jpg"/>
</details>

(在右键菜单的“设置”选项中进行调整) 计时条的颜色将会根据杆种改变。可以在右键菜单中对每种杆种的颜色进行自定义。  

在软件的同目录下放置“轻杆.wav”“中杆.wav”“鱼王杆.wav”文件，则在对应杆种咬钩的同时会播放提示音效
> 压缩包中为范例文件，不放或只放其中1-2个文件也不影响正常使用。

### 鱼眼与特殊天气提示

开启鱼眼后，主界面将会显示鱼眼持续时间倒计时。
海钓中幻海流触发时将会显示幻海流倒计时(120s)。
幻海流持续时间不是固定值，区域倒计时30秒时将会无视剩余时间强制解除幻海流，需要注意。  
当你中途加入已经处于特殊天气的云冠群岛时，第一次的剩余时间计时也是不同步的。后续触发的特殊天气才会开始正常计时。

<details>
<summary>提示样式</summary>
<img width="400" src="https://raw.githubusercontent.com/Natsukage/Assets/main/FishersIntuition/images/FI3.jpg"/>
</details>  

### 自定义

<img width="200" src="https://raw.githubusercontent.com/Natsukage/Assets/main/FishersIntuition/images/FI5.jpg"/>

#### 计时条样式

在设置菜单中可以设置计时条的样式，大小与颜色皆可自行调整。
不知道如何查询颜色hex值可以使用在线取色器，如[HTML 拾色器](https://www.w3cschool.cn/tools/index?name=cpicker)

<details>
<summary>宽500，高40时的效果</summary>
<img width="400" src="https://raw.githubusercontent.com/Natsukage/Assets/main/FishersIntuition/images/FI4.jpg"/>
</details>  

#### 主窗体隐藏

开启**鼠标穿透**选项后，直感的计时条将无法拖动或右键展开菜单，需要修改时需要使用托盘图标右键呼出设置菜单。
开启**未活动时隐藏窗体**选项后，在未抛竿时直感的透明进度条也将会自动隐藏。  
