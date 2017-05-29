#version 330 core

uniform mat4 cameraModel_matrix; //inverse of cameraView_matrix

uniform mat4 object_matrix;

uniform sampler2D sampler;

uniform vec3 lightPosition;

in vec3 fs_position;
varying vec3 fs_normal;
in vec2 fs_texturePos;
in vec3 fs_worldPos;

out vec4 color;

const float r = 0.2f;
const float p = 0.9f;
const float C = 0.5f;
const float pi = 3.141592653589f;

float Z(float beta)
{
	//float c = cos(beta);
	float c = beta;
	float cc = c*c;
	float q = 1 + r*cc - cc;
	return 1/(q*q);
}

float A(float alpha)
{
	float pp = p*p;
	//float c = cos(alpha);
	float c = alpha;
	float cc = c*c;
	return sqrt(p/(pp - pp*cc + cc));
}

float G(float theta)
{
	//float c = cos(theta);
	float c = theta;
	return c/(r - r*c + c);
}

float S(float beta)
{
	return C + (1-C)*pow(1 - beta, 5);//cos(beta)
}

float D(float theta_i, float theta_r, float beta, float alpha)
{
	float G_ti = G(theta_i);
	float G_tr = G(theta_r);

	return ((1 - G_ti * G_tr) / pi) + ((G_ti*G_tr) / (4*pi*cos(theta_i)*cos(theta_r)))*Z(beta)*A(alpha);
}

void main(){
	vec3 viewVec = normalize((cameraModel_matrix * vec4(0, 0, 0, 1)).xyz - fs_worldPos);
	vec3 lightVec = normalize(lightPosition - fs_worldPos);

	float theta_i = max(0.0f, dot(fs_normal, lightVec));
	float theta_r = max(0.0f, dot(fs_normal, viewVec));
	
	vec3 refl = normalize(reflect(-viewVec, fs_normal));//-
	float alpha = max(0.0f, dot(refl, viewVec));

	vec3 H = normalize(viewVec + lightVec);
	float beta = max(0.0f, dot(H, fs_normal));

	vec3 T = cross(vec3(1,0,0), fs_normal);
	vec3 B = cross(T, fs_normal);

	float proc = dot(fs_normal, H);
	vec3 dist = fs_normal * proc;
	vec3 Hs = H - dist;
	float gamma = dot(B, Hs);

	float Schlick = S(beta) + D(theta_i, theta_r, alpha, gamma);
	color = texture(sampler, fs_texturePos) * Schlick;
}
