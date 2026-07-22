using System;

namespace Ffmpegkit.Droid
{
	public partial class FFmpegKitConfig
	{
		/// <summary>
		/// Turns a <c>content://</c> URI into a path FFmpeg can read from.
		/// </summary>
		/// <param name="uri">A URI obtained from the photo picker, a document picker, or a share intent.</param>
		/// <returns>A path to pass to FFmpeg in place of a file name.</returns>
		/// <remarks>
		/// On Android 10 and later an app cannot open arbitrary file paths, and anything the user
		/// picks arrives as a <c>content://</c> URI. FFmpeg has no idea what those are, so passing
		/// one straight into a command fails to open the input. This registers the URI with
		/// FFmpegKit and hands back a path that resolves through the Storage Access Framework:
		/// <code>
		/// var input = FFmpegKitConfig.GetSafParameterForRead(pickedUri);
		/// var session = await FFmpegKit.ExecuteAsync($"-i {input} -c:v mpeg4 \"{outputPath}\"");
		/// </code>
		/// Note the returned value is already a complete argument and must not be quoted.
		/// <para>
		/// The registration lives as long as the process, so treat these as per-operation rather
		/// than obtaining one for every item in a long list.
		/// </para>
		/// Overload of <see cref="GetSafParameterForRead(Android.Content.Context, Android.Net.Uri)"/>
		/// that uses the application context, since callers rarely have a reason to pass another.
		/// </remarks>
		public static string GetSafParameterForRead (Android.Net.Uri uri)
		{
			if (uri is null)
				throw new ArgumentNullException (nameof (uri));

			return GetSafParameterForRead (ApplicationContext, uri);
		}

		/// <summary>Turns a <c>content://</c> URI into a path FFmpeg can write to.</summary>
		/// <remarks>
		/// The document must already exist - the Storage Access Framework has no notion of FFmpeg
		/// creating a file - so create it first with <c>ACTION_CREATE_DOCUMENT</c> and pass the
		/// resulting URI here. See <see cref="GetSafParameterForRead(Android.Net.Uri)"/> for the
		/// rest of the caveats.
		/// </remarks>
		public static string GetSafParameterForWrite (Android.Net.Uri uri)
		{
			if (uri is null)
				throw new ArgumentNullException (nameof (uri));

			return GetSafParameterForWrite (ApplicationContext, uri);
		}

		private static Android.Content.Context ApplicationContext =>
			Android.App.Application.Context
			?? throw new InvalidOperationException (
				"No application context is available. Call the overload that takes a Context explicitly.");
	}
}
