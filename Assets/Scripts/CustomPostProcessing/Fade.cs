using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(FadeRenderer), PostProcessEvent.AfterStack, "Custom/Fade", true)]
public sealed class Fade : PostProcessEffectSettings
{
    public  ColorParameter      color   =   new ColorParameter {value = new Color(0.0f, 0.0f, 0.0f, 1.0f)};

 /*   public override bool IsEnabledAndSupported(PostProcessRenderContext context)
    {
        return enabled.value
            && color.value.a > 0f;
    }*/
}

public sealed class FadeRenderer : PostProcessEffectRenderer<Fade>
{
    public override void Render(PostProcessRenderContext context)
    {
        var sheet = context.propertySheets.Get(Shader.Find("Hidden/Custom/Fade"));
        sheet.properties.SetColor("_Color", settings.color);
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}
