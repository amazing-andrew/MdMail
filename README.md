MdMail
======

MdMail is a command line tool that integrates Markdown + PreMailer + Handlebars.js + Outlook templates for bulding email templates.

You can supply variables to pass to the handlebars.js template.

You can supply a style or css file to apply to your html.

Help
============

    Loads and displays an email template into outlook. Uses an markdown
    markup and data passed into handlebars.js to fill out the template.
    You can also supply a stylesheet to apply to the email.
    
    NOTE: You can set the subject from within the template by having the
    first line start with "Subject:"
    
    
    MdMail "markdown.md" [list of properties]
    
    list of properties: key=value (i.e. user.name=andrew)
    
          --sub, --subject=VALUE sets the subject of the email
      -t, --to=VALUE             sets the to of the email
          --cc=VALUE             sets the to of the email
      --bcc=VALUE            sets the from of the email
          --css, --style=VALUE   sets the file that contains the css style sheet
                                   for the template defaults to (markdown.css)
      -?, --help                 displays this help message



Example:   `mdmail hello.md style=hello.css name.first=Tiny name.last=Tim` 
