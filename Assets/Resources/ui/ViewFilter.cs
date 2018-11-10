using UnityEngine;

namespace UnitySnes
{
	public class ViewFilter : Views
	{
		public void OnTouchNone()
		{
			Frontend.Filter.SetActive(false);
			OnTouchBack();
		}

		public void OnTouchBicubic()
		{
			Frontend.Filter.SetShader(Shader.Find("Pixel Art Filters/Bicubic"));
			OnTouchBack();
		}

		public void OnTouchDdt()
		{
			Frontend.Filter.SetShader(Shader.Find("Pixel Art Filters/DDT"));
			OnTouchBack();
		}

		public void OnTouch2XSal()
		{
			Frontend.Filter.SetShader(Shader.Find("Pixel Art Filters/2xSal"));
			OnTouchBack();
		}

		public void OnTouch2XSalLevel2()
		{
			Frontend.Filter.SetShader(Shader.Find("Pixel Art Filters/2xSal-Level-2"));
			OnTouchBack();
		}
		
		public void OnTouchXbrLevel2Fast()
		{
			Frontend.Filter.SetShader(Shader.Find("Pixel Art Filters/XBR-LV2-Fast"));
			OnTouchBack();
		}

		public void OnTouchXbrLevel2NoBlend()
		{
			Frontend.Filter.SetShader(Shader.Find("Pixel Art Filters/XBR-LV2-NoBlend"));
			OnTouchBack();
		}
		
		public void OnTouchXbrLevel2SmalDetails()
		{
			Frontend.Filter.SetShader(Shader.Find("Pixel Art Filters/XBR-LV2-Small-Details"));
			OnTouchBack();
		}
		
		public void OnTouchXbrLevel3()
		{
			Frontend.Filter.SetShader(Shader.Find("Pixel Art Filters/XBR-LV3"));
			OnTouchBack();
		}
		
		public void OnTouch5Xbr37()
		{
			Frontend.Filter.SetShader(Shader.Find("Pixel Art Filters/5XBR3.7"));
			OnTouchBack();
		}
		
		public void OnTouch2XBrz()
		{
			Frontend.Filter.SetShader(Shader.Find("Pixel Art Filters/2xBRZ"));
			OnTouchBack();
		}
		
		public void OnTouch3XBrz()
		{
			Frontend.Filter.SetShader(Shader.Find("Pixel Art Filters/3xBRZ"));
			OnTouchBack();
		}
		
		public void OnTouch4XBrz()
		{
			Frontend.Filter.SetShader(Shader.Find("Pixel Art Filters/4xBRZ"));
			OnTouchBack();
		}
		
		public void OnTouch5XBrz()
		{
			Frontend.Filter.SetShader(Shader.Find("Pixel Art Filters/5xBRZ"));
			OnTouchBack();
		}
		
		public void OnTouch6XBrz()
		{
			Frontend.Filter.SetShader(Shader.Find("Pixel Art Filters/6xBRZ"));
			OnTouchBack();
		}

		public void OnTouchCrtAperture()
		{
			Frontend.Filter.SetShader(Shader.Find("Pixel Art Filters/CRT Aperture"));
			OnTouchBack();
		}
		
		public void OnTouchCrtCaligari()
		{
			Frontend.Filter.SetShader(Shader.Find("Pixel Art Filters/CRT Caligari"));
			OnTouchBack();
		}
		
		public void OnTouchCrtHyllian()
		{
			Frontend.Filter.SetShader(Shader.Find("Pixel Art Filters/CRT Hyllian"));
			OnTouchBack();
		}
		
		public void OnTouchBack()
		{
			Frontend.OnMenuOpen("ui/settings");
		}
	}
}