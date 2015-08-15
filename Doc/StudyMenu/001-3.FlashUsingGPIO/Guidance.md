# 3. GPIO端子を用いたLEDチカチカ
## いるもの
* PC(Windows or MacOSX)
* USBmicroBケーブル、USB電源
* LANケーブル
* ブレッドボード、ジャンパーワイヤ、LED、抵抗

## 手順
### 1. ワイヤリング。
下図のようにGPIO端子と電子部品をブレッドボードに配線する。  
![Wiring](https://github.com/IsaoNakamura/StudyRPi/blob/wrkFirstPush/Doc/Wiring/outputGPIO/RPi_outputGPIO_bread.png?raw=true)

### 2. 以下のどれかの方法でLEDチカチカする。  
[CASE 0: // コマンド実行でLEDチカチカ](https://github.com/IsaoNakamura/StudyRPi/blob/wrkFirstPush/Doc/StudyMenu/001-3.FlashUsingGPIO/CASE0-usingCmd/Guidance.md)  
~~CASE 1: // シェルスクリプトによるコマンド実行でLEDチカチカ~~  
[CASE 2: // Cコードによるコマンド実行でLEDチカチカ](https://github.com/IsaoNakamura/StudyRPi/blob/wrkFirstPush/Doc/StudyMenu/001-3.FlashUsingGPIO/CASE2-usingCmdByC/Guidance.md)  
[CASE 3: // C++コードによるレジストリ操作でLEDチカチカ ](https://github.com/IsaoNakamura/StudyRPi/blob/wrkFirstPush/Doc/StudyMenu/001-3.FlashUsingGPIO/CASE3-usingRegistByCPP/Guidance.md) 