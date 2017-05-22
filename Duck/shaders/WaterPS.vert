#version 330 core

uniform mat4 cameraModel_matrix; //inverse of cameraView_matrix

uniform mat4 object_matrix;

uniform int isCube;
uniform samplerCube cubeSampler;

in vec3 fs_position;
varying vec3 fs_normal;
in vec3 fs_texturePos;
in vec3 fs_worldPos;

out vec4 color;

vec3 intRey(vec3 pos, vec3 dir)
{
	vec3 t1 = (1 - pos) / dir;
	vec3 t2 = (-1 - pos) / dir;
	vec3 t = max(t1, t2);
	float ts = min(t.x, min(t.y, t.z));
	return pos + ts * dir;
}

float fresnelSchlick(float cosTheta, float f0)
{
	return f0 + (1.0 - f0) * pow(1.0 - cosTheta, 5.0);
}

bool any(vec3 v)
{
	return v.x != 0 ||
		   v.y != 0 ||
		   v.z != 0;
}

vec4 lerp(vec3 refrColor, vec3 reflColor, float f)
{
	return vec4(refrColor * (1 - f) + reflColor * f, 1.0f);
}

void main(){
	if(isCube == 1)
	{
		color = texture(cubeSampler, normalize(fs_texturePos));
		return;
	}


	vec3 normal = normalize(transpose(inverse(mat3(object_matrix))) * fs_normal);
	
	vec3 viewVec = normalize((cameraModel_matrix * vec4(0, 0, 0, 1)).xyz - fs_worldPos);
	float n1 = 1.0f;
    float n2 = 4.0f / 3.0f;
	float ndotv = dot(normal, viewVec);
	float refrIndex = ndotv >= 0 ? n1 / n2 : n2 / n1;
	if (ndotv < 0)
		normal = -normal;

	vec3 refl = reflect(-viewVec, normal);
	vec3 reflColor = texture(cubeSampler, intRey(fs_position, refl)).xyz;
	//color = vec4(reflColor, 0.0f);
	//return;

	vec3 refr = refract(-viewVec, normal, refrIndex);
	vec3 refrColor = texture(cubeSampler, intRey(fs_position, refr)).xyz;
	//color = vec4(refrColor, 0.0f);
	//return;

	float f = any(refr) ? fresnelSchlick(abs(ndotv), 0.17f) : 1.0f;
	color = lerp(refrColor, reflColor, f);
}