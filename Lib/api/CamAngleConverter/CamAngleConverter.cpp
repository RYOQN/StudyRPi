#include <stdio.h>
#include <stdlib.h>
#include <math.h>

#include "CamAngleConverter.h"


DF::CamAngleConverter::CamAngleConverter(
	const int&       sc_width,
	const int&       sc_height,
	const double&    angle_diagonal
	) :
	_sc_width(0),
	_sc_height(0),
	_angle_horz(0.0),
	_angle_vert(0.0),
	_angle_diagonal(0.0),
	_initialized(false)
{
	Initialize(sc_width, sc_height, angle_diagonal);
}

int DF::CamAngleConverter::Initialize(
	const int&       sc_width,
	const int&       sc_height,
	const double&    angle_diagonal
	) {

	_cache.clear();
	_initialized = false;
	_sc_width = _sc_height = 0;
	_angle_horz = _angle_vert = _angle_diagonal = 0.0;

	if (!(IsValid(sc_width) && IsValid(sc_height) && IsValid(angle_diagonal))) {
		return -1;
	}

	double angle_horz, angle_vert;
	int ret = MakeViewAngle(angle_horz, angle_vert, sc_width, sc_height, angle_diagonal);
	if (ret != 0) {
		return ret;
	}

	_sc_width = sc_width;
	_sc_height = sc_height;
	_angle_diagonal = angle_diagonal;
	_angle_horz = angle_horz;
	_angle_vert = angle_vert;
	_initialized = true;
	return ret;
}


// スクリーン座標からカメラのピッチ角とヨー角を算出する 
int DF::CamAngleConverter::ScreenToCameraAngle
	(
		double&       camera_pitch,
		double&       camera_yaw,
		const int&    src_u,
		const int&    src_v
		)
{

	if(_cache.find(XY<int>(src_u,src_v)) != _cache.end()) {
		const XY<double>& py = _cache[XY<int>(src_u, src_v)];
		camera_pitch = py.x;
		camera_yaw   = py.y;
		return 1;
	}
	// 返答領域の初期化 
	camera_pitch = 0.0;
	camera_yaw = 0.0;

	// 入力値チェック 
	if (src_u < 0 && _sc_width < src_u) {
		return -1;
	}
	if (src_v < 0 && _sc_height < src_v) {
		return -1;
	}

	// カメラのピッチ角とヨー角を算出 
	camera_pitch = (static_cast<double>(src_u) / _sc_width * _angle_horz) - (_angle_horz / 2.0);
	camera_yaw = (_angle_vert / 2.0) - (static_cast<double>(src_v) / _sc_height * _angle_vert);

	_cache.insert(std::make_pair(XY<int>(src_u, src_v), XY<double>(camera_pitch, camera_yaw)));

	return 0;
}

// 水平・垂直画角を算出する 
int DF::MakeViewAngle(
	double&       angle_horz,
	double&       angle_vert,
	const int&    sc_width,
	const int&    sc_height,
	const double& angle_diagonal) {

	// 返答領域の初期化 
	angle_horz = 0.0;
	angle_vert = 0.0;

	// d(対角線) 
	double sc_diagonal = sqrt(sc_width*sc_width + sc_height*sc_height);
	if (sc_diagonal <= 1.0e-06) {
		return -1;
	}

	// d比 
	double rate_width = sc_width / sc_diagonal;
	double rate_height = sc_height / sc_diagonal;

	// 水平・垂直画角を算出する 
	angle_horz = angle_diagonal * rate_width;
	angle_vert = angle_diagonal * rate_height;

	return 0;
}

bool DF::IsValid(const double &param) {
	return param > 1.0e-06;
}

bool DF::IsValid(const int &param) {
	return param > 0;
}



int DF::TEST_Simple_CamAngCnv()
{
	const int w = 1280;
	const int h = 720;
	const double ad = 60.0;

	DF::CamAngleConverter camAngCvt(w, h, ad);

	if (camAngCvt.Initialized()) {
		double p, y;
		camAngCvt.ScreenToCameraAngle(p, y, 0, 0);
		printf("%f,%f\n", p, y);
		camAngCvt.ScreenToCameraAngle(p, y, 1280 / 2, 720 / 2);
		printf("%f,%f\n", p, y);
		camAngCvt.ScreenToCameraAngle(p, y, 1280, 720);
		printf("%f,%f\n", p, y);
		camAngCvt.ScreenToCameraAngle(p, y, 500, 59);
		printf("%f,%f\n", p, y);
		if (camAngCvt.ScreenToCameraAngle(p, y, 1280 / 2, 720 / 2) == 1) {
			printf("hit cache↓\n");
		}
		printf("%f,%f\n", p, y);
	}
	
	
	return 0;
}

