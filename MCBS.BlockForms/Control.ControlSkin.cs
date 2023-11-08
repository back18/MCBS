using MCBS.BlockForms.Utility;
using MCBS.Rendering;
using QuanLib.Minecraft.Blocks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace MCBS.BlockForms
{
    public abstract partial class Control
	{
		/// <summary>
		/// 控件外观
		/// </summary>
		public class ControlSkin
		{
			public ControlSkin(Control owner)
			{
				_owner = owner ?? throw new ArgumentNullException(nameof(owner));

				string black = BlockManager.Concrete.Black;
				string white = BlockManager.Concrete.White;
				string gray = BlockManager.Concrete.Gray;

				_ForegroundColor = new HashBlockPixel(black);
				_BackgroundColor = new HashBlockPixel(white);
				_BorderColor = new HashBlockPixel(gray);
				_ForegroundColor_Hover = new HashBlockPixel(black);
				_BackgroundColor_Hover = new HashBlockPixel(white);
				_BorderColor_Hover = new HashBlockPixel(gray);
				_ForegroundColor_Selected = new HashBlockPixel(black);
				_BackgroundColor_Selected = new HashBlockPixel(white);
				_BorderColor_Selected = new HashBlockPixel(gray);
				_ForegroundColor_Hover_Selected = new HashBlockPixel(black);
				_BackgroundColor_Hover_Selected = new HashBlockPixel(white);
				_BorderColor_Hover_Selected = new HashBlockPixel(gray);
			}

			private readonly Control _owner;

			public BlockPixel ForegroundColor
			{
				get => _ForegroundColor;
				set
				{
					if (value is null)
						throw new ArgumentNullException(nameof(value));
					if (!BlockPixel.Equals(_ForegroundColor, value))
					{
						_ForegroundColor = value;
						if (_owner.ControlState == ControlState.None)
							_owner.RequestRendering();
					}
				}
			}
			private BlockPixel _ForegroundColor;

			public BlockPixel BackgroundColor
			{
				get => _BackgroundColor;
				set
				{
					if (value is null)
						throw new ArgumentNullException(nameof(value));
					if (!BlockPixel.Equals(_BackgroundColor, value))
					{
						_BackgroundColor = value;
						if (_owner.ControlState == ControlState.None)
							_owner.RequestRendering();
					}
				}
			}
			private BlockPixel _BackgroundColor;

			public BlockPixel BorderColor
			{
				get => _BorderColor;
				set
				{
					if (value is null)
						throw new ArgumentNullException(nameof(value));
					if (!BlockPixel.Equals(BorderColor, value))
					{
						_BorderColor = value;
						if (_owner.ControlState == ControlState.None)
							_owner.RequestRendering();
					}
				}
			}
			private BlockPixel _BorderColor;

			public BlockPixel ForegroundColor_Hover
			{
				get => _ForegroundColor_Hover;
				set
				{
					if (value is null)
						throw new ArgumentNullException(nameof(value));
					if (!BlockPixel.Equals(_ForegroundColor_Hover, value))
					{
						_ForegroundColor_Hover = value;
						if (_owner.ControlState == ControlState.Hover)
							_owner.RequestRendering();
					}
				}
			}
			private BlockPixel _ForegroundColor_Hover;

			public BlockPixel BackgroundColor_Hover
			{
				get => _BackgroundColor_Hover;
				set
				{
					if (value is null)
						throw new ArgumentNullException(nameof(value));
					if (!BlockPixel.Equals(BackgroundColor_Hover, value))
					{
						_BackgroundColor_Hover = value;
						if (_owner.ControlState == ControlState.Hover)
							_owner.RequestRendering();
					}
				}
			}
			private BlockPixel _BackgroundColor_Hover;

			public BlockPixel BorderColor_Hover
			{
				get => _BorderColor_Hover;
				set
				{
					if (value is null)
						throw new ArgumentNullException(nameof(value));
					if (!BlockPixel.Equals(_BorderColor_Hover, value))
					{
						_BorderColor_Hover = value;
						if (_owner.ControlState == ControlState.Hover)
							_owner.RequestRendering();
					}
				}
			}
			private BlockPixel _BorderColor_Hover;

			public BlockPixel ForegroundColor_Selected
			{
				get => _ForegroundColor_Selected;
				set
				{
					if (value is null)
						throw new ArgumentNullException(nameof(value));
					if (!BlockPixel.Equals(_ForegroundColor_Selected, value))
					{
						_ForegroundColor_Selected = value;
						if (_owner.ControlState == ControlState.Selected)
							_owner.RequestRendering();
					}
				}
			}
			private BlockPixel _ForegroundColor_Selected;

			public BlockPixel BackgroundColor_Selected
			{
				get => _BackgroundColor_Selected;
				set
				{
					if (value is null)
						throw new ArgumentNullException(nameof(value));
					if (!BlockPixel.Equals(_BorderColor_Selected, value))
					{
						_BackgroundColor_Selected = value;
						if (_owner.ControlState == ControlState.Selected)
							_owner.RequestRendering();
					}
				}
			}
			private BlockPixel _BackgroundColor_Selected;

			public BlockPixel BorderColor_Selected
			{
				get => _BorderColor_Selected;
				set
				{
					if (value is null)
						throw new ArgumentNullException(nameof(value));
					if (!BlockPixel.Equals(_BorderColor_Selected, value))
					{
						_BorderColor_Selected = value;
						if (_owner.ControlState == ControlState.Selected)
							_owner.RequestRendering();
					}
				}
			}
			private BlockPixel _BorderColor_Selected;

			public BlockPixel ForegroundColor_Hover_Selected
			{
				get => _ForegroundColor_Hover_Selected;
				set
				{
					if (value is null)
						throw new ArgumentNullException(nameof(value));
					if (!BlockPixel.Equals(_ForegroundColor_Hover_Selected, value))
					{
						_ForegroundColor_Hover_Selected = value;
						if (_owner.ControlState == (ControlState.Hover | ControlState.Selected))
							_owner.RequestRendering();
					}
				}
			}
			private BlockPixel _ForegroundColor_Hover_Selected;

			public BlockPixel BackgroundColor_Hover_Selected
			{
				get => _BackgroundColor_Hover_Selected;
				set
				{
					if (value is null)
						throw new ArgumentNullException(nameof(value));
					if (!BlockPixel.Equals(_BackgroundColor_Hover_Selected, value))
					{
						_BackgroundColor_Hover_Selected = value;
						if (_owner.ControlState == (ControlState.Hover | ControlState.Selected))
							_owner.RequestRendering();
					}
				}
			}
			private BlockPixel _BackgroundColor_Hover_Selected;

			public BlockPixel BorderColor_Hover_Selected
			{
				get => _BorderColor_Hover_Selected;
				set
				{
					if (value is null)
						throw new ArgumentNullException(nameof(value));
					if (!BlockPixel.Equals(_BorderColor_Hover_Selected, value))
					{
						_BorderColor_Hover_Selected = value;
						if (_owner.ControlState == (ControlState.Hover | ControlState.Selected))
							_owner.RequestRendering();
					}
				}
			}
			private BlockPixel _BorderColor_Hover_Selected;

			public Texture? BackgroundTexture
			{
				get => _BackgroundTexture;
				set
				{
					if (value is null)
						throw new ArgumentNullException(nameof(value));
					if (!Texture.Equals(_BackgroundTexture, value))
					{
						_BackgroundTexture = value;
						if (_owner.ControlState == ControlState.None)
							_owner.RequestRendering();
					}
				}
			}
			private Texture? _BackgroundTexture;

			public Texture? BackgroundTexture_Selected
			{
				get => _BackgroundTexture_Selected;
				set
				{
					if (!Texture.Equals(_BackgroundTexture_Selected, value))
					{
						_BackgroundTexture_Selected = value;
						if (_owner.ControlState == ControlState.Hover)
							_owner.RequestRendering();
					}
				}
			}
			private Texture? _BackgroundTexture_Selected;

			public Texture? BackgroundTexture_Hover
			{
				get => _BackgroundTexture_Hover;
				set
				{
					if (value is null)
						throw new ArgumentNullException(nameof(value));
					if (!Texture.Equals(_BackgroundTexture_Hover, value))
					{
						_BackgroundTexture_Hover = value;
						if (_owner.ControlState == ControlState.Selected)
							_owner.RequestRendering();
					}
				}
			}
			private Texture? _BackgroundTexture_Hover;

			public Texture? BackgroundTexture_Hover_Selected
			{
				get => _BackgroundTexture_Hover_Selected;
				set
				{
					if (value is null)
						throw new ArgumentNullException(nameof(value));
					if (!Texture.Equals(_BackgroundTexture_Hover_Selected, value))
					{
						_BackgroundTexture_Hover_Selected = value;
						if (_owner.ControlState == (ControlState.Hover | ControlState.Selected))
							_owner.RequestRendering();
					}
				}
			}
			private Texture? _BackgroundTexture_Hover_Selected;

			public BlockPixel GetForegroundColor(ControlState state)
			{
				return state switch
				{
					ControlState.None => ForegroundColor,
					ControlState.Hover => ForegroundColor_Hover,
					ControlState.Selected => ForegroundColor_Selected,
					ControlState.Hover | ControlState.Selected => ForegroundColor_Hover_Selected,
					_ => throw new InvalidOperationException(),
				};
			}

			public BlockPixel GetBackgroundColor(ControlState state)
			{
				return state switch
				{
					ControlState.None => BackgroundColor,
					ControlState.Hover => BackgroundColor_Hover,
					ControlState.Selected => BackgroundColor_Selected,
					ControlState.Hover | ControlState.Selected => BackgroundColor_Hover_Selected,
					_ => throw new InvalidOperationException(),
				};
			}

			public BlockPixel GetBorderColor(ControlState state)
			{
				return state switch
				{
					ControlState.None => BorderColor,
					ControlState.Hover => BorderColor_Hover,
					ControlState.Selected => BorderColor_Selected,
					ControlState.Hover | ControlState.Selected => BorderColor_Hover_Selected,
					_ => throw new InvalidOperationException(),
				};
			}

			public Texture? GetBackgroundTexture(ControlState state)
			{
				return state switch
				{
					ControlState.None => BackgroundTexture,
					ControlState.Hover => BackgroundTexture_Hover,
					ControlState.Selected => BackgroundTexture_Selected,
					ControlState.Hover | ControlState.Selected => BackgroundTexture_Hover_Selected,
					_ => throw new InvalidOperationException(),
				};
			}

			public void SetForegroundColor(BlockPixel color, ControlState state)
			{
                if (color is null)
					throw new ArgumentNullException(nameof(color));

                color = color.Clone();
                switch (state)
				{
					case ControlState.None:
						ForegroundColor = color;
						break;
					case ControlState.Hover:
						ForegroundColor_Hover = color;
						break;
					case ControlState.Selected:
						ForegroundColor_Selected = color;
						break;
					case ControlState.Hover | ControlState.Selected:
						ForegroundColor_Hover_Selected = color;
						break;
					default:
						throw new InvalidOperationException();
				}
			}

			public void SetBackgroundColor(BlockPixel color, ControlState state)
            {
				if (color is null)
					throw new ArgumentNullException(nameof(color));

                color = color.Clone();
                switch (state)
				{
					case ControlState.None:
						BackgroundColor = color;
						break;
					case ControlState.Hover:
						BackgroundColor_Hover = color;
						break;
					case ControlState.Selected:
						BackgroundColor_Selected = color;
						break;
					case ControlState.Hover | ControlState.Selected:
						BackgroundColor_Hover_Selected = color;
						break;
					default:
						throw new InvalidOperationException();
				}
			}

			public void SetBorderColor(BlockPixel color, ControlState state)
            {
				if (color is null)
					throw new ArgumentNullException(nameof(color));

                color = color.Clone();
                switch (state)
				{
					case ControlState.None:
						BorderColor = color;
						break;
					case ControlState.Hover:
						BorderColor_Hover = color;
						break;
					case ControlState.Selected:
						BorderColor_Selected = color;
						break;
					case ControlState.Hover | ControlState.Selected:
						BorderColor_Hover_Selected = color;
						break;
					default:
						throw new InvalidOperationException();
				}
			}

			public void SetForegroundColor(string color, ControlState state)
			{
				if (color is null)
					throw new ArgumentNullException(nameof(color));

                switch (state)
                {
                    case ControlState.None:
                        ForegroundColor = new HashBlockPixel(color);
                        break;
                    case ControlState.Hover:
                        ForegroundColor_Hover = new HashBlockPixel(color);
                        break;
                    case ControlState.Selected:
                        ForegroundColor_Selected = new HashBlockPixel(color);
                        break;
                    case ControlState.Hover | ControlState.Selected:
                        ForegroundColor_Hover_Selected = new HashBlockPixel(color);
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

			public void SetBackgroundColor(string color, ControlState state)
            {
				if (color is null)
					throw new ArgumentNullException(nameof(color));

                switch (state)
                {
                    case ControlState.None:
                        BackgroundColor = new HashBlockPixel(color);
                        break;
                    case ControlState.Hover:
                        BackgroundColor_Hover = new HashBlockPixel(color);
                        break;
                    case ControlState.Selected:
                        BackgroundColor_Selected = new HashBlockPixel(color);
                        break;
                    case ControlState.Hover | ControlState.Selected:
                        BackgroundColor_Hover_Selected = new HashBlockPixel(color);
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

			public void SetBorderColor(string color, ControlState state)
            {
				if (color is null)
					throw new ArgumentNullException(nameof(color));

                switch (state)
                {
                    case ControlState.None:
                        BorderColor = new HashBlockPixel(color);
                        break;
                    case ControlState.Hover:
                        BorderColor_Hover = new HashBlockPixel(color);
                        break;
                    case ControlState.Selected:
                        BorderColor_Selected = new HashBlockPixel(color);
                        break;
                    case ControlState.Hover | ControlState.Selected:
                        BorderColor_Hover_Selected = new HashBlockPixel(color);
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            public void SetForegroundColor(BlockPixel color, params ControlState[] states)
			{
				foreach (var state in states)
                    SetForegroundColor(color, state);
            }

            public void SetBackgroundColor(BlockPixel color, params ControlState[] states)
            {
                foreach (var state in states)
                    SetBackgroundColor(color, state);
            }

            public void SetBorderColor(BlockPixel color, params ControlState[] states)
            {
                foreach (var state in states)
                    SetBorderColor(color, state);
            }

            public void SetForegroundColor(string color, params ControlState[] states)
            {
                foreach (var state in states)
                    SetForegroundColor(color, state);
            }

            public void SetBackgroundColor(string color, params ControlState[] states)
            {
                foreach (var state in states)
                    SetBackgroundColor(color, state);
            }

            public void SetBorderColor(string color, params ControlState[] states)
            {
                foreach (var state in states)
                    SetBorderColor(color, state);
            }

            public void SetAllForegroundColor(BlockPixel color)
			{
				if (color is null)
					throw new ArgumentNullException(nameof(color));

				ForegroundColor = color.Clone();
				ForegroundColor_Hover = color.Clone();
				ForegroundColor_Selected = color.Clone();
				ForegroundColor_Hover_Selected = color.Clone();
			}

			public void SetAllBackgroundColor(BlockPixel color)
			{
				if (color is null)
					throw new ArgumentNullException(nameof(color));

				BackgroundColor = color.Clone();
				BackgroundColor_Hover = color.Clone();
				BackgroundColor_Selected = color.Clone();
				BackgroundColor_Hover_Selected = color.Clone();
			}

			public void SetAllBorderColor(BlockPixel color)
			{
				if (color is null)
					throw new ArgumentNullException(nameof(color));

				BorderColor = color.Clone();
				BorderColor_Hover = color.Clone();
				BorderColor_Selected = color.Clone();
				BorderColor_Hover_Selected = color.Clone();
			}

			public void SetAllForegroundColor(string color)
			{
				if (color is null)
					throw new ArgumentNullException(nameof(color));

				SetAllForegroundColor(new HashBlockPixel(color));
			}

			public void SetAllBackgroundColor(string color)
			{
				if (color is null)
					throw new ArgumentNullException(nameof(color));

				SetAllBackgroundColor(new HashBlockPixel(color));
			}

			public void SetAllBorderColor(string color)
			{
				if (color is null)
					throw new ArgumentNullException(nameof(color));

				SetAllBorderColor(new HashBlockPixel(color));
			}

			public void SetBackgroundTexture(Texture? texture, ControlState state)
			{
				texture = texture?.Clone();
                switch (state)
				{
					case ControlState.None:
						BackgroundTexture = texture;
						break;
					case ControlState.Hover:
						BackgroundTexture_Hover = texture;
						break;
					case ControlState.Selected:
						BackgroundTexture_Selected = texture;
						break;
					case ControlState.Hover | ControlState.Selected:
						BackgroundTexture_Hover_Selected = texture;
						break;
					default:
						throw new InvalidOperationException();
				}
			}

			public void SetBackgroundTexture<TPixel>(Image<TPixel> image, ControlState state) where TPixel : unmanaged, IPixel<TPixel>
			{
                if (image is null)
					throw new ArgumentNullException(nameof(image));

                switch (state)
                {
                    case ControlState.None:
                        BackgroundTexture = new Texture<TPixel>(image);
                        break;
                    case ControlState.Hover:
                        BackgroundTexture_Hover = new Texture<TPixel>(image);
                        break;
                    case ControlState.Selected:
                        BackgroundTexture_Selected = new Texture<TPixel>(image);
                        break;
                    case ControlState.Hover | ControlState.Selected:
                        BackgroundTexture_Hover_Selected = new Texture<TPixel>(image);
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            public void SetBackgroundTexture(Texture? texture, ControlState[] states)
			{
				foreach (var state in states)
                    SetBackgroundTexture(texture, state);
            }

            public void SetBackgroundTexture<TPixel>(Image<TPixel> image, ControlState[] states) where TPixel : unmanaged, IPixel<TPixel>
            {
                foreach (var state in states)
                    SetBackgroundTexture(image, state);
            }

            public void SetAllBackgroundTexture(Texture? texture)
			{
                BackgroundTexture = texture?.Clone();
				BackgroundTexture_Hover = texture?.Clone();
				BackgroundTexture_Selected = texture?.Clone();
				BackgroundTexture_Hover_Selected = texture?.Clone();
			}

			public void SetAllBackgroundTexture<TPixel>(Image<TPixel> image) where TPixel : unmanaged, IPixel<TPixel>
			{
                if (image is null)
					throw new ArgumentNullException(nameof(image));

				SetAllBackgroundTexture(new Texture<TPixel>(image));
			}
		}
	}
}
