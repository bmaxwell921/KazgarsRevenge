
#region Using Statements
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using SkinnedModelLib;
#endregion

namespace SkinnedAnimationPipeline
{
    /// <summary>
    /// Custom content pipeline processor derives from the built-in
    /// ModelProcessor, extending it to apply an environment mapping
    /// effect to the model as part of the build process.
    /// </summary>
    [ContentProcessor(DisplayName = "Toon Model Processor")]
    public class ToonModelProcessor : ModelProcessor
    {
        /// <summary>
        /// Use our custom EnvironmentMappedMaterialProcessor
        /// to convert all the materials on this model.
        /// </summary>
        protected override MaterialContent ConvertMaterial(MaterialContent material,
                                                         ContentProcessorContext context)
        {
            OpaqueDataDictionary processorParameters = new OpaqueDataDictionary();
            processorParameters["ColorKeyColor"] = ColorKeyColor;
            processorParameters["ColorKeyEnabled"] = ColorKeyEnabled;
            processorParameters["TextureFormat"] = TextureFormat;
            processorParameters["GenerateMipmaps"] = GenerateMipmaps;
            processorParameters["ResizeTexturesToPowerOfTwo"] =
                ResizeTexturesToPowerOfTwo;

            return context.Convert<MaterialContent, MaterialContent>(material,
                "ToonMaterialProcessor", processorParameters);

        }
    }
}
