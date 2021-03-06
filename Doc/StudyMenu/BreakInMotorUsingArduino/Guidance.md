# Arduino(互換機)を用いたミニ四駆用自動モーター慣らし器の開発

## １．はじめに
はじめまして、ISAOXです。  
突然ですが、最近ミニ四駆にハマりました。  
普通に組み立てて遊ぶのにはいいのですが、男はレースとなれば勝ちたくなるもの。  
そこで勝つためのガジェットとして自動モーター慣らし器を開発したので紹介します。  

## ２．モーターの慣らしが必要な理由
モーターをある程度回すと、モーター内部で接触する部分が削れ滑らかになります。  
さらに、電気を流すために接触する部分もフィットするように削れ、電気の流れがよくなります。  
よって、モーターの性能が上がるのです。  

## 3．モーターを慣らす方法
モーター慣らしの方法としては、モーターをシャーシに入れ、駆動部に伝わるギヤなどは外します。  
ギヤを外す理由はモーターに負荷が加わらないようにするためです。  
そして、一定時間回します。  
一定時間というのもミソで長時間回しすぎるのも熱が発生してよくないです。最悪壊れます。  
熱は大敵なので一定時間回したら、冷めるまで待ちます。  
次は電池を逆にして逆回転で同様のことを行います。  
このサイクルを何回か繰り返します。  
そう、手動でね。  

**、、、めんどい。正直めんどいわ！！**

## 4．解決しようとする問題と方法
上記の方法を見ると、手動で同じ事を繰り返す事が大問題です！！  
では、この手動で行っている部分を自動化できるガジェットを開発していきます。  

## 5．設計  
### 5-1. ハードと人を繋ぐIF設計
以下のような簡単なヒューマンインターフェースにします。
* 人が電源を入れたら、ハードがモーターの慣らしを開始。
* ハードがモーターの慣らしを終えたら、人が電源を切る。

### 5-2. ソフトウェア設計
モーター慣らし方法をまとめると以下です。
 1. モーターを一定時間 正回転させる。  
 2. モーターを一定時間 止めて休ませる。  
 3. モーターを一定時間 逆回転させる。  
 4. モーターを一定時間 止めて休ませる。  
 5. 1～4を任意回数行う。  

モーターの状態が遷移していくのが肝です。  
モーターの状態を以下のように定義します。  

| モーターの状態   | 説明                       |
| :------------- | :------------------------ | 
| STOP           | 停止状態(初期状態と最終状態) | 
| FORWARD        | 正回転状態                 | 
| PAUSE_FORWARD  | 正回転後の休止状態          | 
| BACKWARD       | 逆回転状態                 | 
| PAUSE_BACKWARD | 逆回転後の休止状態          | 

上記状態がどういう条件で遷移するかを下記の状態遷移図で定義します。  
![Picture](https://github.com/IsaoNakamura/StudyRPi/blob/wrkDocBreakInMotor/Doc/StudyMenu/BreakInMotorUsingArduino/AutoBreakInMotor_UML-StateMachine.png?raw=true)  
この状態に応じてモーターを制御させるソフトフェアにします。  

各時間は以下のように定義します。  

| 種類        | 時間[msec]  | 使用されるモーターの状態           | 
| :--------- | ----------: | :------------------------------ | 
| 待機時間    |        1000 | STOP                            | 
| 運転時間    |       30000 | FORWARD or BACKWARD             | 
| 冷却時間    |      180000 | PAUSE_FORWARD or PAUSE_BACKWARD | 
上記の時間は適当です。最適な時間を見つけるのも面白いかもしれません。  

### 5-3. ハードウェア設計
#### 5-3-1. モーターの制御
モーターを制御するのに以下のモータードライバICを使用します。  
* [モータードライバIC TA7291P](http://akizukidenshi.com/download/ta7291p.pdf)  
このICは出力電流が少ないので負荷時にモーターが回らなく可能性があります。  
しかし、モーター慣らしは負荷をかけない前提なので今回は良しとしました。  
このICはモーターに直接接続することになります。 

モータードライバICの入力端子に下表のように信号を送ることで、モーターを正回転、逆回転、停止、ブレーキができます。   

|    IN1     |     IN2     |    OUT1    |    OUT2    |     モーターの動き    |
| :--------: | :---------: | :--------: | :--------: |  :----------------: |
|    LOW     |     LOW     |     ∞      |      ∞     |  停止               |
|    HIGH    |     LOW     |    HIGH    |     LOW    |  正回転             |
|    LOW     |     HIGH    |     LOW    |    HIGH    |  逆回転             |
|    HIGH    |     HIGH    |     LOW    |     LOW    |  ブレーキ            |

#### 5-3-2. モータードライバICの制御
モータードライバICへ制御信号を送るのに以下のArduino互換機(マイコン)を使用します。  
* [funduiro pro mini(Arduiro pro mini互換機)](http://ja.aliexpress.com/item/Free-Shipping-new-version-5pcs-lot-Pro-Mini-328-Mini-ATMEGA328-5V-16MHz-for-Arduino/1656644616.html?adminSeq=220352482&shopNumber=1022067)  
このArduinoにモーターの状態に応じてモータードライバICに制御信号を送信するプログラムを書き込むことになります。  

#### 5-3-3. ワイヤリング(配線)
各部品の配線図は以下です。  
![Picture](https://github.com/IsaoNakamura/StudyRPi/blob/wrkDocBreakInMotor/Doc/Wiring/Arduino_AutoBreakInMotorSimple/BreakInMotorSimple_bread.png?raw=true)  

* ArduinoとモータードライバICの接続  
Arduinoのデジタル信号端子をモータードライバICの入力用端子を接続します。  
　⇒ArduinoのデジタルIOピンD8をモータードライバICのIN1へ接続。  
　⇒ArduinoのデジタルIOピンD9をモータードライバICのIN2へ接続。  

* モータードライバICとモーターの接続  
モータードライバICの出力用端子にモーターを接続します。  
　⇒モータードライバICのOUT1をモーターのプラス端子へ接続。  
　⇒モータードライバICのOUT2をモーターのマイナス端子へ接続。  

* 左側にある２本の単三乾電池  
モーターを駆動するための電源です。  
モータードライバICの出力側電源端子(Vs)に接続します。  

* 右側にある４本の単三乾電池  
ArduinoとモータードライバICを動作させるための電源です。  
Arduinoの電源端子(Vcc)とモータードライバICの電源端子(Vcc)にそれぞれ接続します。  

## 6. 製造
### 6-1. コーディング
以下のGitHubにArduinoのSketch(プログラムのソースコード)を格納しました。  
[https://github.com/IsaoNakamura/StudyRPi/blob/master/Sketch/breakInMotorSimple/breakInMotorSimple.ino](https://github.com/IsaoNakamura/StudyRPi/blob/master/Sketch/breakInMotorSimple/breakInMotorSimple.ino)  

### 6-2. Arduinoにプログラムを書き込む
PCとArduinoを専用ケーブルでつなげて、SketchをArduinoに書き込みます。

### 6-3. ブレッドボードにワイヤリングする
配線図通りにブレッドボードに各部品を接続すれば完成です。

## 7. テスト
電池を接続したら、自動モーター慣らし器が動き出します。  
動かなければ、配線が間違っていないかなどをチェックしてください。  
全体の動きを素早くチェックしたい場合は、各動作時間を短くしてみるのもいいですね。  

## 8. 実用化
ここまで説明した内容は自動モーター慣らし機の基本原理でしかありません。  
私が実際にモーター慣らしのためにこのガジェットを開発したので、使いやすく機能を追加しカスタマイズします。  
* モーター慣らしの進捗状況が分かるディスプレイモジュールを追加
* ミニ四駆のシャーシとボディ、パーツケースを筐体として採用
* ユニバーサル基盤に半田付け
* 安全性を考慮し、モーター慣らしが始まるとボディが閉じ、終わるとボディが開く。

各部品の配線図は以下です。  
![Picture](https://github.com/IsaoNakamura/StudyRPi/blob/wrkDocBreakInMotor/Doc/Wiring/Arduino_AutoBreakInMotor/BreakInMotor_bread.png?raw=true)  

動作している時の動画です。  
[![IMAGE ALT TEXT HERE](http://img.youtube.com/vi/9eU1KwwVP6k/0.jpg)](http://www.youtube.com/watch?v=9eU1KwwVP6k)

以下のGitHubに実用化したArduinoのSketch(プログラムのソースコード)を格納しました。  
[https://github.com/IsaoNakamura/StudyRPi/blob/master/Sketch/breakInMotor/breakInMotor.ino](https://github.com/IsaoNakamura/StudyRPi/blob/master/Sketch/breakInMotor/breakInMotor.ino) 


## 9. 注意事項
モーター慣らしをしたとしても確実に速くなるわけではないです。  
逆に遅くなったり、壊れたり、寿命を縮める可能性もありますので自己責任でお願いいたします。

## 10. 最後に
モーターの慣らし方については所説あります。  
モーターの個体差もあいまっているので正解はありません。  
「モーター慣らしは必要ない。練習走行の中で勝手に慣らされて行く」という人もいます。  
自分でこれだという方法を見つけていくのもミニ四駆の楽しさ、奥深さだと思います。  
各自いろいろ試してみましょう。  
