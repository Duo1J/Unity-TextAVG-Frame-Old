Duo1J AVG Frame Demo~

请在Hierarchy中任意物体上添加AVG Frame脚本

AVGFrame.cs-------------------
Image Url Prefix: 图像文件路径前缀， 在Resources文件夹中存放图像文件的前缀
Audio Url Prefix: 同上
TextAsset: 此AVGFrame中的演出脚本

UI Component-----
Canvas: 场景UI画布组件
Text: 对话显示Text组件
Name Text: 角色名称显示Text组件
Character Image: 角色立绘显示Image组件
Background Image: 背景图片显示Image组件
Character Voice: 角色语音Audio Source组件
Background Music: 背景音乐Audio Source组件

----------------------------------------------------------------------------------------------------------------
演出脚本书写格式：
字符要求UTF-8

具体参照  demo.txt

注释: #
脚本中的空行需要#占位

1.文本指令: T/角色名/对话文本/语音/延迟索引:延迟时间/.../ (不定长)
exp: T/Jack/Hello/Jack01.mp3/2:0.5   (在播放到e时候延迟0.5s)
尽量不要连续延迟,如: T/Ketty/12345//2:0.5/3:0.5/4:0.5 可改成 => T/Ketty/12 3 45//2:0.5/4:0.5/6:0.5/

2.系统指令: C/角色img/img位置/x偏移/y偏移/背景img/背景audio/
x,y偏移默认为0，区分正负
img位置只能填入0 1 2,  0-靠左，1-居中，2-靠右
只有audio相关需带上文件后缀

3.选择按钮指令: B/事件名称/选项1/选项2/.../ (不定长)
exp: B/Dowhat/Study/Shop

4.动画指令: A/动画类型/
动画类型不区分大小写
自带动画: 'ch_shake'角色晃动 'bk_shake'背景晃动
exp: A/ch_shake/
自定义动画请重写Duo1JAnimation接口并通过属性访问器注入到AVGFrame

!!!所有指令以/结尾， 若某参数不填则需要//留出
exp: T/Mary/Goodbye!///  语音和延迟均不使用

!!!图片文件不加后缀，音频文件加后缀


interface----------------------------------------------------------------------------------------------------
自行实现了接口后将其挂载在AVGFrame同物体上即可,可参考InterfaceExample.cs

Duo1JAround接口-----------------------------------------
实现void After()和void Before()方法
Before()和After()在播放的开始和结束时候调用

Duo1JAnimation接口-----------------------------------------
实现void StartAnimation(string type)方法
type为 A/type/ 指令中的type

Duo1JAction接口--------------------------------------------
实现void GetButtonChoose(string eventTag, Choose choose)方法
eventTag为 B/event/choose1/choose2/.../指令中的event
Choose.index和Choose.text为选择的索引和内容

Duo1JLoader接口-------------------------------------
实现bool Analysis(out List<AVGModel> modelList, UnityEngine.TextAsset textAsset)方法
返回值为是否解析成功
解析textAsset成AVGModel对象，返回到modelList中

可实现ImageLoad(Async)/AudioLoad(Async)自定义加载
可重写AVGLoader以使用默认Analysis方法



