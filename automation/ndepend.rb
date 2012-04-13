class NDepend
  def create_parameters
    params = []
    params << File.expand_path(@project_file).gsub('/','\\') #NDepend v3 forces absolute windows-path
    return params
  end
end

def setup_ndepend(params={})
	configuration = params[:configuration] || 'Release'
	third_party_path = params[:third_party_path] || '3rdparty'
	project_file = params[:project_file] || FileList.new('*.ndproj').first
	dotnet_framework = params[:dotnet_framework] || 'v4.0'
	namespace :analysis do
		desc 'Run NDepend analysis; see bin/NDependOut/**/NDependReport.html'
		ndepend :ndepend => [:compile] do |nd|
			nd.command = "#{third_party_path}/ndepend-v3/ndepend.console.exe"
			raise ArgumentError, "Project file not found at #{project_file}" unless File.exist? project_file
			project_file = File.expand_path(project_file).gsub('/','\\')
			out_dir = File.expand_path('bin/ndependout/').gsub('/','\\')
			in_dir = File.expand_path(File.join('bin','AnyCPU',dotnet_framework,configuration)).gsub('/','\\')
			nd.project_file = project_file
			nd.parameters << "/outdir #{out_dir} /concurrent /indirs #{in_dir}"
			nd.log_level = :verbose
		end
		task :default => :ndepend
	end
	desc 'Run all static analysis'
	task :analyse => 'analysis:default'
end
