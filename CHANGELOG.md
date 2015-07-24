# 1.0.2
## Change
* Move to Sys.Diag.Trace for logs to remove NLog dependency (so as to not force that on dependents)

# 1.0.0
## Add
* We've been in production with this for a while now.  It deserves the 1.0 version tag.
* Sort out references and dependencies such that nuget is relied upon.

# 0.0.2
## Add
* Ability to prefix each stat, so the prefix/namespace can be supplied one-time via configuration rather than built each time a stat is pushed.  Eg, Mailman would want every stat to be prefixed with `{country}.mailman_{instance position}`

# 0.0.1
* First release
