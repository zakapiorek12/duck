#version 330 core
/*
uniform mat4 projection_matrix;
uniform mat4 cameraview_matrix;
uniform mat4 cameraModel_matrix; //inverse of cameraView_matrix

uniform mat4 object_matrix;

uniform bool drawUnlitScene;

uniform float ambientCoefficient;
uniform vec3 lightPosition;
uniform vec3 lightColor;

uniform float materialSpecExponent;
uniform vec3 specularColor;

uniform int isPlate;
uniform sampler2D tex;

in vec3 fs_position;
in vec3 fs_normal;
in vec2 fs_texturePos;
*/

uniform vec4 surfaceColor;


out vec4 color;

void main(){
	
	color = surfaceColor;
}