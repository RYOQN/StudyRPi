/////////////////////////////////////////////
// this main() is written by isao_nakamura //
/////////////////////////////////////////////
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <math.h>

#include "../../Lib/drivers/JoystickDrv/CJoystickDrv.h"
#include "../../Lib/drivers/SerialDrv/CSerialDrv.h"

#define SERVO_DEG_MIN	(0)
#define SERVO_DEG_MID	(90)
#define SERVO_DEG_MAX	(180)

#define DUALSHOCK_ANALOG_VAL_MAX	( 32767)
#define DUALSHOCK_ANALOG_VAL_MID	(     0)
#define DUALSHOCK_ANALOG_VAL_MIN	(-32767)

#define DUALSHOCK_ANALOG_LEFT_X		(0)
#define DUALSHOCK_ANALOG_LEFT_Y		(1)
#define DUALSHOCK_ANALOG_RIGHT_X	(2)
#define DUALSHOCK_ANALOG_RIGHT_Y	(3)

int main(int argc, char* argv[])
{
	printf("Press Maru-Button to Exit Process.\n");
	int iRet = 0;

	CJoystickDrv*	pJoystick	= NULL;
	CSerialDrv*		pSerial		= NULL;

	try
	{
		// DUALSHOCK LeftJoystick(m_pAxis[0])
		const int axis_min = DUALSHOCK_ANALOG_VAL_MIN;
		const int axis_max = DUALSHOCK_ANALOG_VAL_MAX;
		const int axis_mid = DUALSHOCK_ANALOG_VAL_MID;

		const int servo_mid = SERVO_DEG_MID;
		const int servo_min = SERVO_DEG_MIN;
		const int servo_max = SERVO_DEG_MAX;
		
		// ready Joystick-Obj
		pJoystick = CJoystickDrv::createInstance();
		if(!pJoystick){
			throw 0;
		}
		if(pJoystick->connectJoystick()!=0){
			printf("failed to connectJoystick()\n");
			throw 0;
		}

	
		// ready Serial-Obj
		pSerial = CSerialDrv::createInstance();
		if(!pSerial){
			printf("failed to create pSerial\n");
			throw 0;
		}
	
		printf("begin loop \n");
		while(1){
			// Joystickの状態を更新
			if( pJoystick->readJoystick()!=0 ){
				printf("faile to readJoystick()\n");
				throw 0;
			}
			
			int val_yaw		= servo_mid;
			int val_accel	= servo_mid;
			bool isChanged = false;
			
			if( pJoystick->isChangedAxis(DUALSHOCK_ANALOG_LEFT_X)==1 ){
				int joy_yaw		= pJoystick->getAxisState(DUALSHOCK_ANALOG_LEFT_X);
				if(joy_yaw==axis_mid){ // 中間値
					val_yaw = servo_mid;
				}else if(joy_yaw > axis_mid){ // 右
					double ratio = fabs( (double)joy_yaw / (double)(axis_max) );
					int delta = (int)( (double)( servo_mid - servo_min) * ratio );
					val_yaw = servo_mid - delta;
					//if(75>val_yaw){
					//	val_yaw = 75;
					//}
				}else if(joy_yaw < axis_mid){ // 左
					double ratio = fabs( (double)joy_yaw / (double)(axis_min) );
					int delta = (int)( (double)( servo_max - servo_mid ) * ratio );
					val_yaw = servo_mid + delta;
					//if(105<val_yaw){
					//	val_yaw = 105;
					//}
				}
				isChanged = true;
			}
			
			if( pJoystick->isChangedAxis(DUALSHOCK_ANALOG_RIGHT_Y)==1 ){
				int joy_accel	= pJoystick->getAxisState(DUALSHOCK_ANALOG_RIGHT_Y);
				if(joy_accel==axis_mid){ // 中間値
					val_accel = servo_mid;
				}else if(joy_accel > axis_mid){ // 右
					double ratio = fabs( (double)joy_accel / (double)(axis_max) );
					int delta = (int)( (double)( servo_mid - servo_min) * ratio );
					val_accel = servo_mid - delta;
				}else if(joy_accel < axis_mid){ // 左
					double ratio = fabs( (double)joy_accel / (double)(axis_min) );
					int delta = (int)( (double)( servo_max - servo_mid ) * ratio );
					val_accel = servo_mid + delta;
				}
				isChanged = true;
			}
			
			if(isChanged){

				unsigned char sendBuf[4] = {0};
				sendBuf[0]	= 0x7E;	// 開始デリミタ
				sendBuf[1]	= 0x00;	// FrameType
				sendBuf[2]	= static_cast<unsigned char>(val_yaw);	// ヨー角度
				sendBuf[3]	= static_cast<unsigned char>(val_accel);	// 速度
				
				if( pSerial->sendData(sendBuf, 4) != 0 ){
					throw 0;
				}
				printf("val_yaw=%d(0x%x), val_accel=%d(0x%x)\n",val_yaw,sendBuf[2],val_accel,sendBuf[3]);
			}
			
			//Maru
			if( pJoystick->isChangedButton(JOY_MARU)==1 ){
				if(pJoystick->getButtonState(JOY_MARU) == BUTTON_ON){
					printf("pushed Maru-Button\n");
					break;
				}
			}

			sleep(0);
		}
		printf("end loop\n");
		
		if(pSerial){
			delete pSerial;
			pSerial = NULL;
		}
		if(pJoystick){
			delete pJoystick;
			pJoystick = NULL;
		}
	}
	catch(...)
	{
		printf("catch!! \n");
		iRet = -1;
		if(pSerial){
			delete pSerial;
			pSerial = NULL;
		}
		if(pJoystick){
			delete pJoystick;
			pJoystick = NULL;
		}
	}

	return iRet;
}
