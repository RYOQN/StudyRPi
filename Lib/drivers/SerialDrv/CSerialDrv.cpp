/*
 * CSerialDrv.cpp
 *
 *  Created on: 2015/11/09
 *      Author: isao
 */

#include "CSerialDrv.h"
#include <stdio.h>
#include <unistd.h>

#include <linux/i2c-dev.h>
#include <fcntl.h>
#include <sys/ioctl.h>

#include <stdlib.h>

#include <termios.h>
#include <strings.h>

#define BAUDRATE  B115200

CSerialDrv::CSerialDrv() {
	// TODO 自動生成されたコンストラクター・スタブ
	init();

}

CSerialDrv::~CSerialDrv() {
	// TODO Auto-generated destructor stub
	destroy();
}

CSerialDrv* CSerialDrv::createInstance(	const char*				serialPort/*=SERIAL_PORT*/,
										const unsigned char&	baudrate/*=BAUDRATE*/	)
{
	CSerialDrv* pObj = NULL;
	try
	{
		pObj = new CSerialDrv();
		if(!pObj)
		{
			throw 0;
		}

		if(pObj->startInstance(serialPort, baudrate)!=0)
		{
			throw 0;
		}
	}
	catch(...)
	{
		if(pObj)
		{
			delete pObj;
			pObj = NULL;
		}
	}

	return pObj;
}


void CSerialDrv::init()
{
	m_fd = 0;
	m_address = 0;

	return;
}

void CSerialDrv::destroy()
{
	if( m_fd > 0 )
	{
		close(m_fd);
	}
	return;
}

int CSerialDrv::startInstance(	const char*				serialPort,
								const unsigned char&	baudrate	)
{
	printf("@CSerialDrv::startInstance() start\n");
	int iRet = -1;
	try
	{
		if(!serialPort){
			printf("@CSerialDrv::startInstance() serialPort is NULL\n");
			throw 0;
		}
		m_fd = open(serialPort, O_RDWR );
		if( m_fd < 0 )
		{
			printf("@CSerialDrv::startInstance() failed to open SERIAL_PORT \n");
			throw 0;
		}

		struct termios oldtio;
		struct termios newtio;
		if( ioctl(m_fd, TCGETS, &oldtio) < 0 )
		{
			printf("@CSerialDrv::startInstance() failed to ioctl(TCGETS) \n");
			throw 0;
		}
		bzero(&newtio, sizeof(newtio) );
		newtio = oldtio;
		newtio.c_cflag = ( baudrate | CS8 | CLOCAL | CREAD );
		newtio.c_iflag = ( IGNPAR );
		newtio.c_oflag = 0;
		newtio.c_lflag = ICANON;
		if( ioctl(m_fd, TCSETS, &newtio) < 0 )
		{
			printf("@CSerialDrv::startInstance() failed to ioctl(TCSETS) \n");
			throw 0;
		}

		iRet = 0;
	}
	catch(...)
	{
		if( m_fd > 0 )
		{
			close(m_fd);
		}
		iRet = -1;
	}
	return iRet;
}

int CSerialDrv::receiveData(unsigned char* receiveBuf, int& bufNum)
{
	int iRet = -1;
	try
	{
		if(!receiveBuf){
			throw 0;
		}
		if( bufNum <= 0){
			throw 0;
		}
		
		if (read(m_fd, receiveBuf, bufNum) != bufNum){
			printf("@CSerialDrv::receiveData() Error read from serial\n");
			throw 0;
		}
			
		iRet = 0;
	}
	catch(...)
	{
		iRet = -1;
	}
	return iRet;
}

int CSerialDrv::sendData(const unsigned char* sendBuf, const int& bufNum)
{
	int iRet = -1;
	try
	{
		if(!sendBuf){
			throw 0;
		}
		if( bufNum <= 0){
			throw 0;
		}
		
		if (write(m_fd, sendBuf, bufNum) != bufNum){
			printf("@CSerialDrv::sendData() Error write to serial\n");
			throw 0;
		}
		
		iRet = 0;
	}
	catch(...)
	{
		iRet = -1;
	}
	return iRet;
}