/**
 * @license Copyright (c) 2003-2023, CKSource Holding sp. z o.o. All rights reserved.
 * For licensing, see https://ckeditor.com/legal/ckeditor-oss-license
 */

CKEDITOR.editorConfig = function( config ) {
	// Define changes to default configuration here. For example:
	// config.language = 'fr';
	// config.uiColor = '#AADC6E';
    config.syntaxhighlight_lang = 'csharp';
    config.syntaxhighlight_hideControls = true;
    config.language = 'vi';
    var finderBase = window.appUrl
        ? window.appUrl('/Content/Admin/ckfinder/')
        : CKEDITOR.basePath.replace(/ckeditor\/$/, 'ckfinder/');
    var connector = finderBase + 'core/connector/aspx/connector.aspx';
    config.filebrowserBrowseUrl = finderBase + 'ckfinder.html';
    config.filebrowserImageBrowseUrl = finderBase + 'ckfinder.html?Type=Images';
    config.filebrowserFlashBrowseUrl = finderBase + 'ckfinder.html?Type=Flash';
    config.filebrowserUploadUrl = connector + '?command=QuickUpload&type=Files';
    config.filebrowserImageUploadUrl = connector + '?command=QuickUpload&type=Images';
    config.filebrowserFlashUploadUrl = connector + '?command=QuickUpload&type=Flash';

    if (window.CKFinder && typeof window.CKFinder.setupCKEditor === 'function') {
        window.CKFinder.setupCKEditor(null, finderBase);
    }
};
