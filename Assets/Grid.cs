using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
  // Start is called before the first frame update
  void Start()
  {

  }

  Material lineMaterial;

  void CreateLineMaterial()
  {

    if (!lineMaterial)
    {
      lineMaterial = new Material("Shader \"Lines/Colored Blended\" {" +
          "SubShader { Pass { " +
          "    Blend SrcAlpha OneMinusSrcAlpha " +
          "    ZWrite Off Cull Off Fog { Mode Off } " +
          "    BindChannels {" +
          "      Bind \"vertex\", vertex Bind \"color\", color }" +
          "} } }");
      lineMaterial.hideFlags = HideFlags.HideAndDontSave;
      lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
    }
  }

  // Update is called once per frame
  void Update()
  {
    GL.PushMatrix();
    mat.SetPass(0);
    GL.LoadOrtho();
    GL.Begin(GL.LINES);

    // Set colors and draw verts

    GL.End();
    GL.PopMatrix();
  }
}
