
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Microsoft.Build.Evaluation;

namespace LimbusCompanyWildHunt
{
	public struct Vertex : IVertexType
	{
		private static VertexDeclaration _vertexDeclaration = new VertexDeclaration(new VertexElement[3]
		{
			new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
			new VertexElement(8, VertexElementFormat.Color, VertexElementUsage.Color, 0),
			new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0)
		});

		public Vector2 Position;
		public Color Color;
		public Vector3 TexCoord;

		public Vertex(Vector2 position, Vector3 texCoord, Color color)
		{
			Position = position;
			TexCoord = texCoord;
			Color = color;
		}

		public VertexDeclaration VertexDeclaration
		{
			get => _vertexDeclaration;
		}
	}
}