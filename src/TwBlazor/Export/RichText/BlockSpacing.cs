// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Export.RichText;

/// <summary>
/// The space a text format puts between two blocks.
/// </summary>
internal enum BlockGap
{
    /// <summary>A line break: the next item of the same list.</summary>
    LineBreak,

    /// <summary>A blank line: any other block.</summary>
    BlankLine,

    /// <summary>A blank line between two lists of the same kind, which Markdown would otherwise join into one.</summary>
    ListBreak
}

/// <summary>
/// Decides the space between blocks for the text formats (Markdown and plain text), which show the structure through
/// line breaks rather than markup.
/// </summary>
internal static class BlockSpacing
{
    /// <summary>
    /// Gets the space between two blocks that follow each other.
    /// </summary>
    /// <param name="previous">The block before.</param>
    /// <param name="block">The block after.</param>
    /// <param name="topItem">The last top level list item written before <paramref name="block"/>, if any.</param>
    /// <returns>The space to put between them.</returns>
    public static BlockGap Between(RichTextBlock previous, RichTextBlock block, RichTextBlock? topItem)
    {
        var bothItems = previous.Kind == RichTextBlockKind.ListItem
            && block.Kind == RichTextBlockKind.ListItem
            && previous.QuoteDepth == block.QuoteDepth;
        if (!bothItems)
        {
            return BlockGap.BlankLine;
        }

        if (block.ListLevel > 0 || block.ListId == topItem?.ListId)
        {
            return BlockGap.LineBreak;
        }

        return topItem?.Ordered == block.Ordered ? BlockGap.ListBreak : BlockGap.BlankLine;
    }

    /// <summary>
    /// Gets the last top level list item once <paramref name="block"/> has been written.
    /// </summary>
    /// <param name="block">The block just written.</param>
    /// <param name="topItem">The last top level list item before it, if any.</param>
    /// <returns><paramref name="block"/> when it is a top level list item, otherwise <paramref name="topItem"/>.</returns>
    public static RichTextBlock? TopItemAfter(RichTextBlock block, RichTextBlock? topItem) =>
        block is { Kind: RichTextBlockKind.ListItem, ListLevel: 0 } ? block : topItem;
}
