#version 330 core

uniform mat4 cameraModel_matrix; //inverse of cameraView_matrix

uniform mat4 object_matrix;

uniform sampler2D sampler;

in vec3 fs_position;
varying vec3 fs_normal;
in vec2 fs_texturePos;
in vec3 fs_worldPos;

out vec4 color;

void main(){
	color = texture(sampler, fs_texturePos);
}