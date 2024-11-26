# Unity-Text-Extraction-Tool

## Unity文字字符提取工具
可以提取出Unity项目中，Asset文件夹下的所有预制体和场景(场景需要放入Building Setting)中的Text和TextMeshPro组件里的text中的字符内容
可以更改工具脚本中的正则表达式过滤规则来自定义提取出项目中自己需要的字符

目前设定的工具提取字符规则为：
1.提取出汉字字符
2.提取出常用的全角字符
3.过滤英文数字和半角字符
4.过滤换行符
5.不会出现相同的字符，已经提取的字符不会再提取\n

### 输出文件为.txt，可自定义输出路径
