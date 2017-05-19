#version 330 core

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

uniform int isCube;
uniform samplerCube cubeSampler;

in vec3 fs_position;
varying vec3 fs_normal;
in vec3 fs_texturePos;
in vec3 fs_worldPos;

uniform vec4 surfaceColor;

out vec4 color;

vec3 intRey(vec3 pos, vec3 dir)
{
	vec3 t1 = (1 - pos) / dir;
	vec3 t2 = (-1 - pos) / dir;
	vec3 t = max(t1, t2);
	float ts = min(t.x, min(t.y, t.z));
	return pos + ts * dir;
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
	vec3 res = intRey(fs_position, refl);
	vec3 reflColor = texture(cubeSampler, normalize(res)).xyz;
	color = vec4(reflColor, 0.0f);
	return;


	vec3 surfacePos = vec3(object_matrix * vec4(fs_position, 1));
	vec3 surfaceToLight = normalize(lightPosition - surfacePos);
	vec3 surfaceToCamera = normalize((cameraModel_matrix * vec4(0.0, 0.0, 0.0, 1.0)).xyz - surfacePos);

	//ambient
    vec3 ambient = ambientCoefficient * lightColor * surfaceColor.xyz;
	vec3 diffuse = vec3(0.0, 0.0, 0.0);
	vec3 specular = vec3(0.0, 0.0, 0.0);

	//diffuse
	float diffuseCoefficient = max(0.0, dot(normal, surfaceToLight));
	diffuse = diffuseCoefficient * specularColor * lightColor;
    
	//specular
	float specularCoefficient = 0.0;
	if(diffuseCoefficient > 0.0)
		specularCoefficient = pow(max(0.0, dot(surfaceToCamera, reflect(-surfaceToLight, normal))), materialSpecExponent);
	specular = specularCoefficient * specularColor * lightColor;
	
	color = vec4(ambient + diffuse + specular, surfaceColor.a);
}