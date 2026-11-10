# Native-command error handling is stable starting with PowerShell 7.4.
$ErrorActionPreference = 'Stop'
if ($PSVersionTable.PSVersion -lt [version]'7.4')
{
    throw 'Script steps require PowerShell 7.4 or newer.'
}
$PSNativeCommandUseErrorActionPreference = $true
[Console]::InputEncoding = [System.Text.UTF8Encoding]::new($false)
[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new($false)
$OutputEncoding = [Console]::OutputEncoding
$global:LASTEXITCODE = 0
try
{
    # Parse the step separately so param and using declarations retain their meaning.
    $step = [scriptblock]::Create([Console]::In.ReadToEnd())
    & $step | Out-Default
    exit $LASTEXITCODE
}
catch [System.Management.Automation.NativeCommandExitException]
{
    [Console]::Error.WriteLine($_)
    exit $_.Exception.ExitCode
}
